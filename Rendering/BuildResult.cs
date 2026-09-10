using System.Windows.Media.Media3D;

namespace FluffiesEngine.Rendering;

/// <summary>A finished character model plus its triangle count (shared by all builders).</summary>
public readonly record struct BuildResult(Model3DGroup Model, int TriangleCount);
