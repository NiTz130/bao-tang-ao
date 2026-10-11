#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace Museum.Tests
{
    public class FirstPersonControllerTests : InputTestFixture
    {
        GameObject root;
        InputActionAsset actions;
        CursorLockMode savedLock;
        bool savedVisible;

        FirstPersonController Rig()
        {
            savedLock = Cursor.lockState;
            savedVisible = Cursor.visible;
            root = new GameObject("Player");
            root.SetActive(false);
            root.transform.position = Vector3.up;
            var pivot = new GameObject("Pivot").transform;
            pivot.SetParent(root.transform, false);
            var player = root.AddComponent<FirstPersonController>();
            actions = Object.Instantiate(AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions"));
            typeof(FirstPersonController).GetField("actions", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(player, actions);
            typeof(FirstPersonController).GetField("cameraPivot", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(player, pivot);
            root.SetActive(true);
            return player;
        }

        static void Freeze(FirstPersonController player)
        {
            var property = typeof(FirstPersonController).GetProperty("Frozen");
            Assert.IsNotNull(property, "Frozen must exist");
            property.SetValue(player, true);
        }

        [TearDown] public override void TearDown()
        {
            if (root != null) Object.DestroyImmediate(root);
            if (actions != null) Object.DestroyImmediate(actions);
            Cursor.lockState = savedLock;
            Cursor.visible = savedVisible;
            base.TearDown();
        }

        [UnityTest] public IEnumerator Frozen_IgnoresMoveAndLook()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var mouse = InputSystem.AddDevice<Mouse>();
            var player = Rig();
            Freeze(player);
            var start = root.transform.position;
            var rotation = root.transform.rotation;
            var pitch = root.transform.GetChild(0).localRotation;
            Press(keyboard.wKey);
            for (float elapsed = 0; elapsed < 0.3f; elapsed += Time.unscaledDeltaTime)
            {
                Set(mouse.delta, new Vector2(20, 10));
                yield return null;
            }
            Assert.AreEqual(start.x, root.transform.position.x, 1e-4f);
            Assert.AreEqual(start.z, root.transform.position.z, 1e-4f);
            Assert.Less(Quaternion.Angle(rotation, root.transform.rotation), 1e-4f);
            Assert.Less(Quaternion.Angle(pitch, root.transform.GetChild(0).localRotation), 1e-4f);
        }

        // Wait for a condition instead of a fixed time: the first frames of a cold run can be very slow, and Unity clamps their delta time.
        static IEnumerator WaitUntil(System.Func<bool> condition, float timeoutSeconds = 5f)
        {
            float start = Time.realtimeSinceStartup;
            while (!condition() && Time.realtimeSinceStartup - start < timeoutSeconds) yield return null;
        }

        [UnityTest] public IEnumerator Frozen_StillAppliesGravity()
        {
            var player = Rig();
            Freeze(player);
            float y = root.transform.position.y;
            yield return WaitUntil(() => root.transform.position.y < y - 0.1f);
            Assert.Less(root.transform.position.y, y - 0.1f);
        }

        [UnityTest] public IEnumerator NotFrozen_MovesWithW()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            Rig();
            float z = root.transform.position.z;
            Press(keyboard.wKey);
            yield return WaitUntil(() => root.transform.position.z > z + 0.1f);
            Assert.Greater(root.transform.position.z, z + 0.1f);
        }
    }
}
#endif
