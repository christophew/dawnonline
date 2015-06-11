# Introduction #

How can an agent manipulate the world?

An agent has (currently) a very limited amount of output options:
  * Turn: turn left/right
  * Thrust: apply forward/backward force


# ICreature #

```
   public interface ICreature : IEntity
    {
       ...

        void Turn(double percent);
        void Thrust(double percent);
    }
```