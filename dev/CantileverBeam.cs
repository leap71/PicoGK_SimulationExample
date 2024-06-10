using System.Numerics;
using PicoGK;


namespace Leap71
{
    using ShapeKernel;

    namespace Simulation
    {
        public class CantileverBeam
        {

            /// <summary>
            /// Cantilever beam example.
            /// </summary>
            public static void WriteCantileverBeam()
            {
                // physical inputs
                float fSolidDensity = 7800f;                                 // kg/m3
                float fSolidYoungModulus = 200f * MathF.Pow(10f, 9f);        // Pa
                float fSolidPoissonRatio = 0.3f;                             // -


                // geometric
                BaseBox oBox = new BaseBox(new LocalFrame(new Vector3(0, 0, 0)), 10f, 30f, 10f); //  z: 0.1, x: 0.3, y: 0.1 in m
                Voxels voxSolidDomain = oBox.voxConstruct();
                Sh.PreviewVoxels(voxSolidDomain, Cp.clrGray);


                // fixed patch
                // todo: simple cylinder that overlaps with the wheel's central hub
                // todo: only matches sizes for voxel size = 0.3mm
                BaseBox oFixed = new BaseBox(new LocalFrame(new Vector3(-15f, 0, 0)), 10f, 1f, 10f);
                Voxels voxFixedPatch = oFixed.voxConstruct();
                Sh.PreviewVoxels(voxFixedPatch, Cp.clrRed, 0.6f);


                // forced patch as side box
                // todo: simple box that overlaps with the wheel's side
                // todo: only matches sizes for voxel size = 0.3mm
                BaseBox oLoad = new BaseBox(new LocalFrame(new Vector3(15f, 0, 0f)), -1f, 1f, 10f);
                Voxels voxForcePatch = oLoad.voxConstruct();
                Sh.PreviewVoxels(voxForcePatch, Cp.clrBlue, 0.6f);


                // create VDB file from input data
                string strVDBFilePath = Sh.strGetExportPath(Sh.EExport.VDB, "CantileverBeam");
                SimpleMechSimulationOutput oOutput = new(strVDBFilePath,
                                                            fSolidDensity,
                                                            fSolidPoissonRatio,
                                                            fSolidYoungModulus,
                                                            voxSolidDomain,
                                                            voxFixedPatch,
                                                            voxForcePatch);
                Library.Log("Finished Task.");
                return;
            }
        }
    }
}