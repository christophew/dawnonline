# Input sensors #

How does an agent perceive the environment?

An agent has a number of input sensors to their disposal:
  * IEye: visual recognition sensor
  * IEye as nose
  * IBumper: collision detection sensor

## IEye ##

```
    public interface IEye
    {
        double DistanceToFirstVisible(List<IEntity> sortedEntities, bool useLineOfSight = true);
        double WeightedDistanceToFirstVisible(List<IEntity> sortedEntities, bool useLineOfSight = true);

        bool SeesCreature(ICreature creature);
        bool SeesACreature(List<CreatureTypeEnum> species, IEntity spawnPointToExclude);
        bool SeesACreature(List<CreatureTypeEnum> species);
        bool SeesACreature(CreatureTypeEnum specy);

        bool SeesAnObstacle(EntityTypeEnum entityType);
    }

```

The most useful member function for the neural agents is: **WeightedDistanceToFirstVisible**
It will return the distance to the first visible entity of a specific type in the visual reach of the sensor. The result is weigthed so it can easily be used as input for a neural network.

![https://docs.google.com/drawings/d/1fmpAWc9vAEhYBsAKkaPDsrW2Y57f178gT_KHmNQ2h0U/pub?w=683&h=269&nonsense=dummy.jpg](https://docs.google.com/drawings/d/1fmpAWc9vAEhYBsAKkaPDsrW2Y57f178gT_KHmNQ2h0U/pub?w=683&h=269&nonsense=dummy.jpg)


## Nose ##

By slightly changing the use of an IEye, it can also be used as a nose to retrieve the general direction of the home SpawnPoint.

= by ignoring the line-of-sight rules

```
var distance = _nose.WeightedDistanceToFirstVisible(spawnpoint, false);
```

![https://docs.google.com/drawings/d/1BtDsL1CRyrIrriEvKdgIPUgGzEopYm385SBInroqjxc/pub?w=318&h=338&nonsense=dummy.jpg](https://docs.google.com/drawings/d/1BtDsL1CRyrIrriEvKdgIPUgGzEopYm385SBInroqjxc/pub?w=318&h=338&nonsense=dummy.jpg)

## IBumper ##

```
    public interface IBumper
    {
        bool Hit { get; }

        void Clear();
    }
```

A bumper works in cooperation with the underlying physics engine.
We attach a special FarseerFixture in front of the creature to handle collision detection:

```
        internal Bumper(Creature creature, Vector2 offset)
        {
            _fixture = FixtureFactory.AttachCircle(_radius, 0, creature.Place.Fixture.Body, offset, this);
            _fixture.IsSensor = true;
            _fixture.OnCollision += OnCollision;
        }

```

![https://docs.google.com/drawings/d/1KFcjSdnQoIhgrKxXyD3GGiDxArtzC81dwA8I_lcaIH0/pub?w=437&h=273&nonsense=dummy.jpg](https://docs.google.com/drawings/d/1KFcjSdnQoIhgrKxXyD3GGiDxArtzC81dwA8I_lcaIH0/pub?w=437&h=273&nonsense=dummy.jpg)