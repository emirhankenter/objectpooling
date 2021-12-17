

# Object Pooling SDK
Object Pooling is a great way to optimize your projects and lower the burden that is placed on the CPU when having to rapidly create and destroy GameObjects.

- Generic type objects
- Particle System

#### Table of Contents
- [Requirements](#requirements)
- [Installation](#installation)
- [Usage](#usage)

## Requirements
- Unity 2019.4 or newer

## Installation
There is only one method of installation available for the Object Pooling SDK:

[comment]: <> (<details>)
<summary><b>Unity Package Manager</b></summary>

1. From within the Unity Editor navigate to **Edit > Project Settings** and then to the **Package Manager** settings tab.

   ![unity registry manager](Documentation/package_manager_tab.png)

2. Create a *New Scoped Registry* by entering
    ```
    Name        npmjs
    URL         https://registry.npmjs.org
    Scope(s)    com.pika
    ```
   and click **Save**.
3. Open the **Window > Package Manager** and switch to **My Registries** via the **Packages:** dropdown menu. You will see the Object Pooling SDK package available
   on which you can then click **Install** for the platforms you would like to include. Dependencies will be added automatically.

   > *Depending on your project configuration and if you are upgrading from a previous version, some of these steps may already be marked as "completed"*

   ![my registries menu selection](Documentation/registry_menu.png)

[comment]: <> (</details>)


## Usage

```C#
// You can spawn objects just by one code line like this
ObjectPooling.Instance.Spawn(prefab);
// Or
prefab.Spawn();

// Once you want to save the object to the pool, you need to call recycle method
ObjectPooling.Instance.Recycle(prefab)
// Or
prefab.Recycle();

// You can pass parameters to the Spawn method like 
// Transform as parent
// Vector3 as global position
// Quaternion as global rotation
prefab.Spawn(parent, position, rotation)
```
####Particle Pooling

To use particles with this system, you need to do two steps in target prefab:
1. Set "Stop Action" to "CallBack" in particle system
2. Add Component PooledParticleObject

![my registries menu selection](Documentation/pooled_particle_object.png)

<br>
<br>

####IRecycleCallbackReceiver
This interface help you to receive a callback when object has been recycle 
```C#
    public class TestObject : MonoBehaviour, IRecycleCallbackReceiver
    {
        public void OnRecycle()
        {
            Debug.Log("Test object has been recycled");
        }
    }
```

## License
[Modified MIT License](LICENSE)