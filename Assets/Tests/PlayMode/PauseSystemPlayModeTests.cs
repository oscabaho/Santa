using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Cysharp.Threading.Tasks;

public class PauseSystemPlayModeTests
{
    private GameObject go;

    [SetUp]
    public void Setup()
    {
        go = new GameObject("PauseTestHarness");
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void TogglePauseChangesTimeScale()
    {
        // Arrange
        float before = Time.timeScale;
        Assert.AreEqual(1f, before, "Expected default timeScale 1 before pause.");

        // Act
        Time.timeScale = 0f;

        // Assert
        Assert.AreEqual(0f, Time.timeScale, "Expected timeScale 0 after pause.");

        // Resume
        Time.timeScale = 1f;
        Assert.AreEqual(1f, Time.timeScale, "Expected timeScale 1 after resume.");
    }
}
