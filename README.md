# UnityEditor integration for LeoEcsLite C# Entity Component System framework

Based on the standard integration for LeoEcsLite (https://github.com/Leopotam/ecslite-unityeditor), this fork adds a manual renaming capability that is used if the unity debug systems are instructed NOT to bake component names.  Can be enabled/disabled on a world-by-world basis.

```cs
//To enable, simply instruct the World Debug system that it should NOT bake components into the name
//(baking the component names provides the default behavior seen in the original unityeditor integration.
.Add (new EcsWorldDebugSystem(bakeComponentsInName:false))
```

Then, using the extension methods, the user can manually name entities:
```cs
//naming on creation
_gameWorld.NewEntity("Flagship");

...

//renaming later, based on status change
_gameWorld.UpdateEntityName(entity, "Flagship (dying)");
```


# License
The software is released under the terms of the [MIT license](./LICENSE.md).

No personal support or any guarantees.
