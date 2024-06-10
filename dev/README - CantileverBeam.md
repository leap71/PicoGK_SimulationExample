# Simulation Setup with PicoGK for a Cantilever beam

## Why Canilever beam?

A well understood example, so easy to test/debug.

## Problem defenition

![Canitlever Beam problem defenition](Documentation/CantileverPD.png)

A line load to be applied in negative Y direction.

ToDo:
- `SimpleMechSimulationOutput`: I was not sure how to 'overwrite' this class for my Cantilever beam exactly. ToDo
- `convert .vdb`: Write a script to convert .vdb to my C++ Immersed Finite Element code
- Solve FEA
- Convert results back to .vdb
- Update ReadTask in PicoGK

Comments/Questions:
- `m_oForceField`: I would call m_oForceField: m_oExternalForceField, for example, because the internal end external force vector.
       
