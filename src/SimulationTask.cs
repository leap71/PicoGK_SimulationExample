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


using System.Numerics;
using PicoGK;


namespace Leap71
{
    using ShapeKernel;

    namespace Simulation
    {
        public class SimulationSetup
        {
            /// <summary>
            /// Creates all the data necessary for a simple fluid simulation setup.
            /// Creates and exports everything in a single VDB file.
            /// </summary>
            public static void WriteFluidSimulationTask()
            {
                // physical inputs
                float fFluidDensity         = 1000f;            // kg/m3
                float fFluidViscosity       = 0.00000897f;      // m2/s
                float fFluidInletVelocity   = 1.5f;             // m/s


                // geometric inputs
                SimpleFlowDevice oPipe      = new SimpleFlowDevice();
                Voxels voxFluidDomain       = oPipe.voxGetFluidDomain();
                Voxels voxSolidDomain       = oPipe.voxGetSolidDomain();
                Voxels voxInletPatch        = oPipe.voxGetInletPatch();


                // create VDB file from input data
                string strVDBFilePath       = Sh.strGetExportPath(Sh.EExport.VDB, "SimpleFluidSimulation");
                SimpleFluidSimulationOutput oOutput = new(  strVDBFilePath,
                                                            fFluidDensity,
                                                            fFluidViscosity,
                                                            fFluidInletVelocity,
                                                            voxFluidDomain,
                                                            voxSolidDomain,
                                                            voxInletPatch);
                
                Library.Log("Finished Task.");
                return;
            }

            /// <summary>
            /// Imports a VDB file.
            /// Retrieves all the simple fluid simulation input data.
            /// </summary>
            public static void ReadFluidSimulationTask()
            {
                // load VDB file and retreive simulation inputs
                string strVDBFilePath                   = Sh.strGetExportPath(Sh.EExport.VDB, "SimpleFluidSimulation");
                SimpleFluidSimulationInput oData        = new(strVDBFilePath);


                // get data
                Voxels voxFluidDomain                   = oData.voxGetFluidDomain();
                Voxels voxSolidDomain                   = oData.voxGetSolidDomain();
                ScalarField oDensityField               = oData.oGetDensityField();
                ScalarField oViscosityField             = oData.oGetViscosityField();
                VectorField oVelocityField              = oData.oGetVelocityField();


                // get bounding box and probe fluid domain values
                // use your own resolution / step length
                BBox3 oBBox                     = Sh.oGetBoundingBox(voxFluidDomain);
                float fStep                     = 2f;
                for (float fZ = oBBox.vecMin.Z; fZ <= oBBox.vecMax.Z; fZ += fStep)
                {
                    for (float fX = oBBox.vecMin.X; fX <= oBBox.vecMax.X; fX += fStep)
                    {
                        for (float fY = oBBox.vecMin.Y; fY <= oBBox.vecMax.Y; fY += fStep)
                        {
                            Vector3 vecPosition = new Vector3(fX, fY, oBBox.vecMax.Z - fZ);

                            //query density
                            bool bSuccess = oDensityField.bGetValue(vecPosition, out float fFieldValue);
                            if (bSuccess == true)
                            {
                                float fDensityValue = fFieldValue;
                                // todo: do something with the value...
                            }

                            //query viscosity
                            bSuccess = oViscosityField.bGetValue(vecPosition, out fFieldValue);
                            if (bSuccess == true)
                            {
                                float fViscosity = fFieldValue;
                                // todo: do something with the value...
                            }

                            //query velocity
                            bSuccess = oVelocityField.bGetValue(vecPosition, out Vector3 vecFieldValue);
                            if (bSuccess == true)
                            {
                                Vector3 vecVelocity = vecFieldValue;
                                // todo: do something with the value...

                                ColorScale3D oScale = new ColorScale3D(new RainboxSpectrum(), 0f, 1.5f);
                                ColorFloat clr      = oScale.clrGetColor(vecVelocity.Length());
                                PolyLine oPoly      = new(clr);
                                oPoly.nAddVertex(vecPosition);
                                oPoly.AddCross(0.4f * fStep);
                                Library.oViewer().Add(oPoly);
                            }
                        }
                    }
                }

                // previews
                Sh.PreviewVoxels(voxFluidDomain, Cp.clrBlue);
                Sh.PreviewVoxels(voxSolidDomain, Cp.clrRock);

                Library.Log("Finished Task.");
                return;
            }

            /// <summary>
            /// Creates all the data necessary for a simple mechanical simulation setup.
            /// Creates and exports everything in a single VDB file.
            /// </summary>
            public static void WriteMechanicalSimulationTask()
            {
                // physical inputs
                float fSolidDensity         = 7800f;                            // kg/m3
                float fSolidYoungModulus    = 200f * MathF.Pow(10f, 9f);        // Pa
                float fSolidPoissonRatio    = 0.3f;                             // -


                // geometric inputs
                // load rover wheel as external geometry in VDB format
                // todo: replace through computational geometry, to have more robustness across voxel sizes
                string strCurrentFolder     = Directory.GetCurrentDirectory();
                strCurrentFolder            = Directory.GetParent(strCurrentFolder)?.FullName;
                strCurrentFolder            = Directory.GetParent(strCurrentFolder)?.FullName;
                strCurrentFolder            = Directory.GetParent(strCurrentFolder)?.FullName;
                strCurrentFolder            = Path.Combine(strCurrentFolder, "dev");
                string strSolidDomainPath   = Path.Combine(strCurrentFolder, "RoverWheel_02.VDB");

                Voxels voxSolidDomain       = Voxels.voxFromVdbFile(strSolidDomainPath);
                Sh.PreviewVoxels(voxSolidDomain, Cp.clrGray);


                // fixed patch as centre cylinder
                // todo: simple cylinder that overlaps with the wheel's central hub
                // todo: only matches sizes for voxel size = 0.3mm
                BaseCylinder oCyl           = new BaseCylinder(new LocalFrame(new Vector3(0, 0, -25f)), 50, 25f);
                Voxels voxFixedPatch        = oCyl.voxConstruct();
                Sh.PreviewVoxels(voxFixedPatch, Cp.clrRed, 0.6f);


                // forced patch as side box
                // todo: simple box that overlaps with the wheel's side
                // todo: only matches sizes for voxel size = 0.3mm
                BaseBox oBox = new BaseBox(new LocalFrame(new Vector3(75, 0, -25f)), 50, 20f, 80f);
                Voxels voxForcePatch        = oBox.voxConstruct();
                Sh.PreviewVoxels(voxForcePatch, Cp.clrBlue, 0.6f);


                // create VDB file from input data
                string strVDBFilePath       = Sh.strGetExportPath(Sh.EExport.VDB, "SimpleMechSimulation");
                SimpleMechSimulationOutput oOutput = new(  strVDBFilePath,
                                                            fSolidDensity,
                                                            fSolidPoissonRatio,
                                                            fSolidYoungModulus,
                                                            voxSolidDomain,
                                                            voxFixedPatch,
                                                            voxForcePatch);
                Library.Log("Finished Task.");
                return;
            }

            /// <summary>
            /// Imports a VDB file.
            /// Retrieves all the simple mechanical simulation input data.
            /// </summary>
            public static void ReadMechanicalSimulationTask()
            {
                // load VDB file and retreive simulation inputs
                string strVDBFilePath                   = Sh.strGetExportPath(Sh.EExport.VDB, "SimpleMechSimulation");
                SimpleMechSimulationInput oData         = new(strVDBFilePath);


                // get data
                Voxels voxSolidDomain                   = oData.voxGetSolidDomain();
                VectorField oForceField                 = oData.oGetForceField();
                VectorField oDisplacementField          = oData.oGetDisplacementField();
                ScalarField oDensityField               = oData.oGetDensityField();
                ScalarField oModulusField               = oData.oGetYoungModulusField();
                ScalarField oPoissonField               = oData.oGetPoissonRatioField();


                // get bounding box and probe fluid domain values
                // use your own resolution / step length
                BBox3 oBBox                     = Sh.oGetBoundingBox(voxSolidDomain);
                float fStep                     = 2f;
                for (float fZ = oBBox.vecMin.Z; fZ <= oBBox.vecMax.Z; fZ += fStep)
                {
                    for (float fX = oBBox.vecMin.X; fX <= oBBox.vecMax.X; fX += fStep)
                    {
                        for (float fY = oBBox.vecMin.Y; fY <= oBBox.vecMax.Y; fY += fStep)
                        {
                            Vector3 vecPosition = new Vector3(fX, fY, oBBox.vecMax.Z - fZ);

                            //query density
                            bool bSuccess = oDensityField.bGetValue(vecPosition, out float fFieldValue);
                            if (bSuccess == true)
                            {
                                float fDensityValue = fFieldValue;
                                // todo: do something with the value...
                            }

                            //query young's modulus
                            bSuccess = oModulusField.bGetValue(vecPosition, out fFieldValue);
                            if (bSuccess == true)
                            {
                                float fYoungModulus = fFieldValue;
                                // todo: do something with the value...
                            }

                            //query poisson's ratio
                            bSuccess = oPoissonField.bGetValue(vecPosition, out fFieldValue);
                            if (bSuccess == true)
                            {
                                float fPoissonRatio = fFieldValue;
                                // todo: do something with the value...
                            }

                            //query displacement
                            bSuccess = oDisplacementField.bGetValue(vecPosition, out Vector3 vecFieldValue);
                            if (bSuccess == true)
                            {
                                Vector3 vecDisplacement = vecFieldValue;
                                // todo: do something with the value...
                            }

                            //query force
                            bSuccess = oForceField.bGetValue(vecPosition, out vecFieldValue);
                            if (bSuccess == true)
                            {
                                Vector3 vecForce = vecFieldValue;
                                // todo: do something with the value...
                            }
                        }
                    }
                }

                // previews
                Sh.PreviewVoxels(voxSolidDomain, Cp.clrRock);

                Library.Log("Finished Task.");
                return;
            }

            /// <summary>
            /// Test function for the mesh displacement.
            /// </summary>
            public static void MeshDisplacementTask()
            {
                // load solid domain
                string strCurrentFolder         = Directory.GetCurrentDirectory();
                strCurrentFolder                = Directory.GetParent(strCurrentFolder)?.FullName;
                strCurrentFolder                = Directory.GetParent(strCurrentFolder)?.FullName;
                strCurrentFolder                = Directory.GetParent(strCurrentFolder)?.FullName;
                strCurrentFolder                = Path.Combine(strCurrentFolder, "dev");
                string strSolidDomainPath       = Path.Combine(strCurrentFolder, "RoverWheel_02.VDB");

                Voxels voxSolidDomain           = Voxels.voxFromVdbFile(strSolidDomainPath);

                // generate dummy displacement vector field
                VectorField oDisplacementField  = VectorFieldUtil.oGetDummyVectorField(voxSolidDomain);

                // write VDB file
                string strVDBFilePath           = Sh.strGetExportPath(Sh.EExport.VDB, "DummyDisplacement");
                OpenVdbFile oFile               = new();
                oFile.nAdd(voxSolidDomain,          $"Simulation.Domain_{SimulationKeyWords.m_strSolidKey}");
                oFile.nAdd(oDisplacementField,      $"Simulation.Field_{SimulationKeyWords.m_strDisplacementKey}");
                oFile.SaveToFile(strVDBFilePath);
                Library.Log($"Exported VdbFile {strVDBFilePath} successfully.");

                new DisplacementCheck(voxSolidDomain, oDisplacementField, 20f);

                Sh.PreviewVoxels(voxSolidDomain, Cp.clrGray, 0.7f);
                Library.Log("Finished Task.");
                return;
            }
        }
    }
}