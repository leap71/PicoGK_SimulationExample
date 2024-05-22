using Leap71.Simulation;


try
{
    PicoGK.Library.Go(
        0.3f,
        SimulationSetup.WriteTask
        //SimulationSetup.ReadTask
        );
}
catch (Exception e)
{
    Console.WriteLine("Failed to run Task.");
    Console.WriteLine(e.ToString());
}