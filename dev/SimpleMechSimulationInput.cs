//
// SPDX-License-Identifier: CC0-1.0
//
// This example code file is released to the public under Creative Commons CC0.
// See https://creativecommons.org/publicdomain/zero/1.0/legalcode
//
// To the extent possible under law, LEAP 71 has waived all copyright and
// related or neighboring rights to this PicoGK example code file.
//
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//


using PicoGK;


namespace Leap71
{
    namespace Simulation
    {
        public class SimpleMechSimulationInput
        {
            // solid part
            protected ScalarField   m_oSolidDensityField;       // density in kg/m3
            protected ScalarField   m_oSolidPoissonField;       // poisson's ration in -
            protected ScalarField   m_oSolidModulusField;       // young's modulus in Pa
            protected VectorField   m_oForceField;              // force in N
            protected VectorField   m_oDisplacementField;       // displacement in m
            protected Voxels        m_voxSolidDomain;


            /// <summary>
            /// Class to load, analyse the specified VDB file and retrieve input data for a simulation setup.
            /// The simulation is a simple fluid flow without heat transfer.
            /// Class retieves a fluid and solid domain and fields for velocity, viscosity and density.
            /// </summary>
            public SimpleMechSimulationInput(string strVDBFilePath)
            {
                // load 
                OpenVdbFile vdbfileRead = new(strVDBFilePath);
                Library.Log($"Loaded VdbFile {strVDBFilePath}");


                // check if content is as expected
                // 1x voxel field is expected
                // 3x scalar field is expected
                // 2x vector field is expected
                uint nNumberOfVoxelFields   = 0;
                uint nNumberOfVectorFields  = 0;
                uint nNumberOfScalarFields  = 0;
                uint nNumberOfFields        = (uint)vdbfileRead.nFieldCount();
                Library.Log($"VdbFile contains {nNumberOfFields} fields");
                if (nNumberOfFields != 6)
                {
                    throw new Exception(
                        "Six fields are expected. " +
                        "VDB file content is not suitable for this simulation input.");
                }

                for (int nField = 0; nField < nNumberOfFields; nField++)
                {
                    string strType = vdbfileRead.strFieldType(nField);
                    string strName = vdbfileRead.strFieldName(nField);
                    Library.Log($"-  Field {nField} has type {strType} and name '{strName}'");

                    OpenVdbFile.EFieldType eType = vdbfileRead.eFieldType(nField);
                    if (eType == OpenVdbFile.EFieldType.VectorField)
                    {
                        nNumberOfVectorFields++;
                    }
                    else if (eType == OpenVdbFile.EFieldType.Voxels)
                    {
                        nNumberOfVoxelFields++;
                    }
                    else if (eType == OpenVdbFile.EFieldType.ScalarField)
                    {
                        nNumberOfScalarFields++;
                    }
                    else
                    {
                        throw new Exception(
                            "Unsupported field found. " +
                            "VDB file content is not suitable for this simulation input.");
                    }
                }
                if ((nNumberOfVectorFields != 2) ||
                    (nNumberOfVoxelFields  != 1) ||
                    (nNumberOfScalarFields != 3))
                {
                    throw new Exception(
                        "Two vector fields are expected. " +
                        "One voxel field is expected. " +
                        "Three scalar fields are expected. " +
                        "VDB file content is not suitable for this simulation input.");
                }


                // retrieve fields
                for (int nField = 0; nField < nNumberOfFields; nField++)
                {
                    string strName = vdbfileRead.strFieldName(nField);
                    OpenVdbFile.EFieldType eType = vdbfileRead.eFieldType(nField);

                    // solid domain voxel field
                    if ((eType == OpenVdbFile.EFieldType.Voxels) &&
                        (strName.Contains(SimulationKeyWords.m_strSolidKey) == true))
                    {
                        m_voxSolidDomain = vdbfileRead.voxGet(nField);
                        Library.Log($"-  Field '{strName}' successfully retrieved as solid domain voxel field.");
                    }

                    // displacement vector field
                    else if ((eType == OpenVdbFile.EFieldType.VectorField) &&
                             (strName.Contains(SimulationKeyWords.m_strDisplacementKey) == true))
                    {
                        m_oDisplacementField = vdbfileRead.oGetVectorField(nField);
                        Library.Log($"-  Field '{strName}' successfully retrieved as displacement vector field.");
                    }

                    // force vector field
                    else if ((eType == OpenVdbFile.EFieldType.VectorField) &&
                             (strName.Contains(SimulationKeyWords.m_strForceKey) == true))
                    {
                        m_oForceField = vdbfileRead.oGetVectorField(nField);
                        Library.Log($"-  Field '{strName}' successfully retrieved as force vector field.");
                    }

                    // density scalar field
                    else if ((eType == OpenVdbFile.EFieldType.ScalarField) &&
                             (strName.Contains(SimulationKeyWords.m_strDensityKey) == true))
                    {
                        m_oSolidDensityField = vdbfileRead.oGetScalarField(nField);
                        Library.Log($"-  Field '{strName}' successfully retrieved as density scalar field.");
                    }

                    // young's modulus scalar field
                    else if ((eType == OpenVdbFile.EFieldType.ScalarField) &&
                             (strName.Contains(SimulationKeyWords.m_strModulusKey) == true))
                    {
                        m_oSolidModulusField = vdbfileRead.oGetScalarField(nField);
                        Library.Log($"-  Field '{strName}' successfully retrieved as young's modulus scalar field.");
                    }

                    // poisson's ratio scalar field
                    else if ((eType == OpenVdbFile.EFieldType.ScalarField) &&
                             (strName.Contains(SimulationKeyWords.m_strPoissonKey) == true))
                    {
                        m_oSolidPoissonField = vdbfileRead.oGetScalarField(nField);
                        Library.Log($"-  Field '{strName}' successfully retrieved as poisson's ratio scalar field.");
                    }
                }

                if (m_oForceField == null)
                    throw new MissingFieldException("Missing force field");

                if (m_oDisplacementField == null)
                    throw new MissingFieldException("Missing displacement field");

                if (m_oSolidDensityField == null)
                    throw new MissingFieldException("Missing solid density field");

                if (m_oSolidModulusField == null)
                    throw new MissingFieldException("Missing solid young's modulus field");

                if (m_oSolidPoissonField == null)
                    throw new MissingFieldException("Missing solid poisson's ratio field");

                if (m_voxSolidDomain == null)
                    throw new MissingFieldException("Missing solid domain voxel field");
            }

            /// <summary>
            /// Returns the solid domain as a voxel field.
            /// </summary>
            public Voxels voxGetSolidDomain()
            {
                return m_voxSolidDomain;
            }

            /// <summary>
            /// Returns the displacements as a vector field.
            /// All values are specified in m.
            /// </summary>
            public VectorField oGetDisplacementField()
            {
                return m_oDisplacementField;
            }

            /// <summary>
            /// Returns the forces as a vector field.
            /// All values are specified in N.
            /// </summary>
            public VectorField oGetForceField()
            {
                return m_oForceField;
            }

            /// <summary>
            /// Returns the solid's densities as a scalar field.
            /// All values are specified in kg/m3.
            /// </summary>
            public ScalarField oGetDensityField()
            {
                return m_oSolidDensityField;
            }

            /// <summary>
            /// Returns the young's moduli as a scalar field.
            /// All values are specified in Pa.
            /// </summary>
            public ScalarField oGetYoungModulusField()
            {
                return m_oSolidModulusField;
            }

            /// <summary>
            /// Returns the poisson ratios as a scalar field.
            /// All values are specified in -.
            /// </summary>
            public ScalarField oGetPoissonRatioField()
            {
                return m_oSolidPoissonField;
            }
        }
    }
}