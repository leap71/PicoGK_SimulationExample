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
        public static class SimulationKeyWords
        {
            public readonly static string m_strFluidKey     = "fluid";
            public readonly static string m_strSolidKey     = "solid";
            public readonly static string m_strDensityKey   = "density";
            public readonly static string m_strViscosityKey = "viscosity";
            public readonly static string m_strVelocityKey  = "velocity";
        }

        public class SimpleFluidSimulationInput
        {
            // fluid
            protected ScalarField   m_oFluidDensityField;       // density in kg/m3
            protected ScalarField   m_oFluidViscosityField;     // kin. viscosity in m2/s
            protected VectorField   m_oFluidVelocityField;      // flow speed in m/s
            protected Voxels        m_voxFluidDomain;

            // solid part
            protected Voxels        m_voxSolidDomain;


            /// <summary>
            /// Class to load, analyse the specified VDB file and retrieve input data for a simulation setup.
            /// The simulation is a simple fluid flow without heat transfer.
            /// Class retieves a fluid and solid domain and fields for velocity, viscosity and density.
            /// </summary>
            public SimpleFluidSimulationInput(string strVDBFilePath)
            {
                // load 
                OpenVdbFile vdbfileRead = new(strVDBFilePath);
                Library.Log($"Loaded VdbFile {strVDBFilePath}");


                // check if content is as expected
                // 2x voxel field is expected
                // 2x scalar field is expected
                // 1x vector field is expected
                uint nNumberOfVoxelFields   = 0;
                uint nNumberOfVectorFields  = 0;
                uint nNumberOfScalarFields  = 0;
                uint nNumberOfFields        = (uint)vdbfileRead.nFieldCount();
                Library.Log($"VdbFile contains {nNumberOfFields} fields");
                if (nNumberOfFields != 5)
                {
                    throw new Exception(
                        "Five fields are expected. " +
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
                if ((nNumberOfVectorFields != 1) ||
                    (nNumberOfVoxelFields  != 2) ||
                    (nNumberOfScalarFields != 2))
                {
                    throw new Exception(
                        "One vector field is expected. " +
                        "Two voxel fields are expected. " +
                        "Two scalar fields are expected. " +
                        "VDB file content is not suitable for this simulation input.");
                }


                // retrieve fields
                for (int nField = 0; nField < nNumberOfFields; nField++)
                {
                    string strName = vdbfileRead.strFieldName(nField);
                    OpenVdbFile.EFieldType eType = vdbfileRead.eFieldType(nField);

                    // fluid domain voxel field
                    if ((eType == OpenVdbFile.EFieldType.Voxels) &&
                        (strName.Contains(SimulationKeyWords.m_strFluidKey) == true))
                    {
                        m_voxFluidDomain = vdbfileRead.voxGet(nField);
                        Library.Log($"-  Field '{strName}' successfully retrieved as fluid domain voxel field.");
                    }

                    // solid domain voxel field
                    else if ((eType == OpenVdbFile.EFieldType.Voxels) &&
                             (strName.Contains(SimulationKeyWords.m_strSolidKey) == true))
                    {
                        m_voxSolidDomain = vdbfileRead.voxGet(nField);
                        Library.Log($"-  Field '{strName}' successfully retrieved as solid domain voxel field.");
                    }

                    // velocity vector field
                    else if ((eType == OpenVdbFile.EFieldType.VectorField) &&
                             (strName.Contains(SimulationKeyWords.m_strVelocityKey) == true))
                    {
                        m_oFluidVelocityField = vdbfileRead.oGetVectorField(nField);
                        Library.Log($"-  Field '{strName}' successfully retrieved as velocity vector field.");
                    }

                    // density scaler field
                    else if ((eType == OpenVdbFile.EFieldType.ScalarField) &&
                             (strName.Contains(SimulationKeyWords.m_strDensityKey) == true))
                    {
                        m_oFluidDensityField = vdbfileRead.oGetScalarField(nField);
                        Library.Log($"-  Field '{strName}' successfully retrieved as density scalar field.");
                    }

                    // density scaler field
                    else if ((eType == OpenVdbFile.EFieldType.ScalarField) &&
                             (strName.Contains(SimulationKeyWords.m_strViscosityKey) == true))
                    {
                        m_oFluidViscosityField = vdbfileRead.oGetScalarField(nField);
                        Library.Log($"-  Field '{strName}' successfully retrieved as viscosity scalar field.");
                    }
                }

                if (m_oFluidDensityField == null)
                    throw new MissingFieldException("Missing fluid density field");

                if (m_oFluidVelocityField == null)
                    throw new MissingFieldException("Missing fluid velocity field");

                if (m_oFluidViscosityField == null)
                    throw new MissingFieldException("Missing fluid viscosity field");

                if (m_voxFluidDomain == null)
                    throw new MissingFieldException("Missing fluid domain voxel field");

                if (m_voxSolidDomain == null)
                    throw new MissingFieldException("Missing solid domain voxel field");
            }

            /// <summary>
            /// Returns the fluid domain as a voxel field.
            /// </summary>
            public Voxels voxGetFluidDomain()
            {
                return m_voxFluidDomain;
            }

            /// <summary>
            /// Returns the solid domain as a voxel field.
            /// </summary>
            public Voxels voxGetSolidDomain()
            {
                return m_voxSolidDomain;
            }

            /// <summary>
            /// Returns the fluid flow speeds / velocities as a vector field.
            /// All values are specified in m/s.
            /// </summary>
            public VectorField oGetVelocityField()
            {
                return m_oFluidVelocityField;
            }

            /// <summary>
            /// Returns the fluid densities as a scalar field.
            /// All values are specified in kg/m3.
            /// </summary>
            public ScalarField oGetDensityField()
            {
                return m_oFluidDensityField;
            }

            /// <summary>
            /// Returns the fluid (kinematic) viscosities as a scalar field.
            /// All values are specified in m2/s.
            /// </summary>
            public ScalarField oGetViscosityField()
            {
                return m_oFluidViscosityField;
            }
        }
    }
}