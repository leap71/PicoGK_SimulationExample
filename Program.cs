using Leap71.Simulation;


try
{
    PicoGK.Library.Go(
        0.3f,

        //SimulationSetup.WriteFluidSimulationTask
        //SimulationSetup.ReadFluidSimulationTask

        //SimulationSetup.MeshDisplacementTask
        //SimulationSetup.WriteMechanicalSimulationTask
        //SimulationSetup.ReadMechanicalSimulationTask

        CantileverBeam.WriteCantileverBeam

        );
}
catch (Exception e)
{
    Console.WriteLine("Failed to run Task.");
    Console.WriteLine(e.ToString());
}