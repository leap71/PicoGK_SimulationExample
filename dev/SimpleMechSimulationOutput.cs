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
using System.Numerics;


namespace Leap71
{
    using ShapeKernel;

    namespace Simulation
    {
        public class SimpleMechSimulationOutput
        {
            // solid part
            protected ScalarField   m_oSolidDensityField;       // density in kg/m3
            protected ScalarField   m_oSolidPoissonField;       // poisson's ration in -
            protected ScalarField   m_oSolidModulusField;       // young's modulus in Pa
            protected VectorField   m_oForceField;              // force in N
            protected VectorField   m_oDisplacementField;       // displacement in m
            protected Voxels        m_voxSolidDomain;


            /// <summary>
            /// Class to create a VDB file from physical and geometric input data.
            /// This information can be used for automated simulation setup.
            /// </summary>
            public SimpleMechSimulationOutput(  string strVDBFilePath,
                                                float  fSolidDensity,
                                                float  fSolidPoissonRatio,
                                                float  fSolidYoungModulus,
                                                Voxels voxSolidDomain,
                                                Voxels voxFixedDisplacementPatch,
                                                Voxels voxAppliedForcePatch)
            {
                // set domains
                m_voxSolidDomain            = voxSolidDomain;


                //todo: set voxFixedDisplacementPatch to not null
                //todo: generate displacement patch
                //todo: fix displacement to zero in all voxels inside the patch?
                voxFixedDisplacementPatch = Sh.voxIntersect(m_voxSolidDomain, voxFixedDisplacementPatch);
                Vector3 vecFixed            = new Vector3(0, 0, 0);
                VectorField oFixedField     = new VectorField(voxFixedDisplacementPatch, vecFixed);


                //todo: merge with displacement vector field for entire solid domain
                //todo: Vectores vecFixed and vecDefault are the same. More then one vector field needed?
                Vector3 vecDefault          = new Vector3(0, 0, 0);
                m_oDisplacementField        = new VectorField(m_voxSolidDomain, vecDefault);
                VectorFieldMerge.Merge(oFixedField, m_oDisplacementField);


                //todo: set voxAppliedForcePatch to not null
                //todo: generate force patch
                //todo: assume force as 20 N in x-dir
                //todo: apply force vector to all voxels inside the patch?
                voxAppliedForcePatch        = Sh.voxIntersect(m_voxSolidDomain, voxAppliedForcePatch);
                Vector3 vecForce            = new Vector3(20, 0, 0); 
                VectorField oForcedField    = new VectorField(voxAppliedForcePatch, vecForce);


                //todo: merge with force vector field for entire solid domain
                m_oForceField               = new VectorField(m_voxSolidDomain, vecDefault);
                VectorFieldMerge.Merge(oForcedField, m_oForceField);


                // density scalar field from voxel field
                m_oSolidDensityField = ScalarFieldUtil.oGetConstScalarField(m_oDisplacementField, fSolidDensity);


                // modulus scalar field from voxel field
                m_oSolidModulusField = ScalarFieldUtil.oGetConstScalarField(m_oDisplacementField, fSolidYoungModulus);


                // poisson scalar field from voxel field
                m_oSolidPoissonField = ScalarFieldUtil.oGetConstScalarField(m_oDisplacementField, fSolidPoissonRatio);


                // write VDB file
                OpenVdbFile oFile = new();
                oFile.nAdd(m_voxSolidDomain,        $"Simulation.Domain_{SimulationKeyWords.m_strSolidKey}");
                oFile.nAdd(m_oDisplacementField,    $"Simulation.Field_{SimulationKeyWords.m_strDisplacementKey}");
                oFile.nAdd(m_oForceField,           $"Simulation.Field_{SimulationKeyWords.m_strForceKey}");
                oFile.nAdd(m_oSolidDensityField,    $"Simulation.Field_{SimulationKeyWords.m_strDensityKey}");
                oFile.nAdd(m_oSolidModulusField,    $"Simulation.Field_{SimulationKeyWords.m_strModulusKey}");
                oFile.nAdd(m_oSolidPoissonField,    $"Simulation.Field_{SimulationKeyWords.m_strPoissonKey}");
                oFile.SaveToFile(strVDBFilePath);
                Library.Log($"Exported VdbFile {strVDBFilePath} successfully.");
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