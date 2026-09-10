using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using FluffiesEngine.Models;
using HelixToolkit.Wpf;

namespace FluffiesEngine.Rendering;

/// <summary>
/// Builds a stylised anthro furry out of primitive solids (ellipsoids, spheres and capsules)
/// from a <see cref="CharacterParameters"/> snapshot. Pure function — no UI state.
/// </summary>
public static class FurryCharacterBuilder
{
    public static BuildResult Build(CharacterParameters p)
    {
        var group = new Model3DGroup();
        int tris = 0;

        // Shared materials -------------------------------------------------
        var fur = Fur(p.FurPrimary, p.Glossiness);
        var accent = Fur(p.FurSecondary, p.Glossiness);
        var dark = Fur(Color.FromRgb(0x1A, 0x17, 0x1F), 0.35);
        var eyeWhite = Glossy(Color.FromRgb(0xF6, 0xF4, 0xFA));
        var iris = Glossy(p.EyeColor);
        var horn = Fur(Color.FromRgb(0xE9, 0xE1, 0xCE), 0.5);

        // Local helper that bakes a mesh into the group and tallies triangles.
        void Add(MeshBuilder mb, Material material, Transform3D? transform = null)
        {
            var mesh = mb.ToMesh();
            mesh.Freeze();
            tris += mesh.TriangleIndices.Count / 3;
            var model = new GeometryModel3D(mesh, material) { BackMaterial = material };
            if (transform is not null) model.Transform = transform;
            group.Children.Add(model);
        }

        MeshBuilder New() => new(true, false);

        // Proportions ------------------------------------------------------
        double height = p.Height;
        double build = p.Build;
        double headScale = p.HeadSize;
        double limb = p.LimbLength;

        double legLen = 0.62 * limb;
        double hipY = legLen;
        double torsoH = 0.70 * height;
        double torsoTopY = hipY + torsoH;
        double torsoCenterY = hipY + torsoH * 0.5;
        double torsoRx = 0.34 * build;
        double torsoRy = torsoH * 0.52;
        double torsoRz = 0.27 * build;

        double headRx = 0.30 * headScale;
        double headRy = 0.31 * headScale;
        double headRz = 0.30 * headScale;
        double neckY = torsoTopY;
        double headCenterY = neckY + headRy * 0.85;

        // ---- Contact shadow (flat disc on the ground) --------------------
        {
            var mb = New();
            mb.AddEllipsoid(new Point3D(0, 0.004, 0.04), torsoRx * 1.5, 0.004, torsoRz * 2.4, 36, 6);
            var shadowBrush = new SolidColorBrush(Color.FromArgb(0x66, 0, 0, 0));
            shadowBrush.Freeze();
            var shadow = new DiffuseMaterial(shadowBrush);
            Add(mb, shadow);
        }

        // ---- Torso + belly ----------------------------------------------
        {
            var mb = New();
            mb.AddEllipsoid(new Point3D(0, torsoCenterY, 0), torsoRx, torsoRy, torsoRz, 28, 22);
            // Chest taper toward the neck.
            mb.AddEllipsoid(new Point3D(0, torsoTopY - torsoRy * 0.35, 0), torsoRx * 0.82, torsoRy * 0.5, torsoRz * 0.82, 26, 18);
            Add(mb, fur);

            var bellyMb = New();
            bellyMb.AddEllipsoid(new Point3D(0, torsoCenterY - torsoRy * 0.08, torsoRz * 0.5),
                torsoRx * 0.6, torsoRy * 0.78, torsoRz * 0.62, 24, 18);
            Add(bellyMb, accent);
        }

        // ---- Neck --------------------------------------------------------
        {
            var mb = New();
            mb.AddCylinder(new Point3D(0, neckY - 0.05, 0), new Point3D(0, headCenterY - headRy * 0.6, 0),
                torsoRx * 0.78, 22);
            Add(mb, fur);
        }

        // ---- Head + muzzle ----------------------------------------------
        double snout = p.SnoutLength;
        double muzzleLen = 0.08 + 0.24 * snout;
        double muzzleZ = headRz * 0.55 + muzzleLen * 0.5;
        double muzzleY = headCenterY - headRy * 0.28;
        {
            var mb = New();
            mb.AddEllipsoid(new Point3D(0, headCenterY, 0), headRx, headRy, headRz, 30, 24);
            if (snout > 0.05)
            {
                mb.AddEllipsoid(new Point3D(0, muzzleY, muzzleZ),
                    headRx * 0.46, headRy * 0.36, muzzleLen * 0.75 + 0.05, 24, 18);
            }
            Add(mb, fur);

            // Muzzle underside / chin patch in the accent colour.
            var chinMb = New();
            chinMb.AddEllipsoid(new Point3D(0, muzzleY - headRy * 0.12, muzzleZ),
                headRx * 0.34, headRy * 0.2, muzzleLen * 0.65 + 0.04, 20, 14);
            Add(chinMb, accent);
        }

        // ---- Nose --------------------------------------------------------
        {
            double noseZ = headRz * 0.55 + muzzleLen + 0.02;
            var mb = New();
            mb.AddSphere(new Point3D(0, muzzleY + headRy * 0.04, noseZ), headRx * 0.12, 18, 14);
            Add(mb, dark);
        }

        // ---- Eyes --------------------------------------------------------
        {
            double eyeY = headCenterY + headRy * 0.14;
            double eyeX = headRx * 0.44;
            double eyeZ = headRz * 0.74;
            double eyeR = headRx * 0.2;

            var whiteMb = New();
            var irisMb = New();
            var pupilMb = New();
            foreach (int s in new[] { -1, 1 })
            {
                whiteMb.AddSphere(new Point3D(s * eyeX, eyeY, eyeZ), eyeR, 18, 16);
                irisMb.AddSphere(new Point3D(s * eyeX, eyeY, eyeZ + eyeR * 0.52), eyeR * 0.66, 16, 14);
                pupilMb.AddSphere(new Point3D(s * eyeX, eyeY, eyeZ + eyeR * 0.82), eyeR * 0.32, 14, 12);
            }
            Add(whiteMb, eyeWhite);
            Add(irisMb, iris);
            Add(pupilMb, dark);
        }

        // ---- Ears (and optional inner-ear) -------------------------------
        {
            double earX = headRx * 0.62;
            double earBaseY = headCenterY + headRy * 0.66;
            double earZ = -headRz * 0.02;
            double ear = p.EarSize;

            foreach (int s in new[] { -1, 1 })
            {
                (double rx, double ry, double rz, double tiltZ, double tiltX) = p.EarShape switch
                {
                    EarShape.Round => (headRx * 0.30 * ear, headRx * 0.30 * ear, headRx * 0.16, 14.0, 8.0),
                    EarShape.Long => (headRx * 0.17 * Math.Min(ear, 1.6), headRy * 0.95 * ear, headRx * 0.14, 9.0, 6.0),
                    _ => (headRx * 0.21 * ear, headRy * 0.6 * ear, headRx * 0.15, 20.0, 10.0), // Pointed
                };

                var outer = New();
                outer.AddEllipsoid(new Point3D(0, 0, 0), rx, ry, rz, 18, 16);
                Add(outer, fur, EarTransform(s, rx: rx, ry: ry, earX, earBaseY, earZ, tiltZ, tiltX, push: 0));

                var inner = New();
                inner.AddEllipsoid(new Point3D(0, 0, 0), rx * 0.6, ry * 0.7, rz * 0.5, 16, 14);
                Add(inner, accent, EarTransform(s, rx: rx, ry: ry, earX, earBaseY, earZ, tiltZ, tiltX, push: rz * 0.6));
            }
        }

        // ---- Horns (optional) -------------------------------------------
        if (p.Horns)
        {
            double hornX = headRx * 0.46;
            double hornBaseY = headCenterY + headRy * 0.72;
            double hornZ = headRz * 0.08;
            foreach (int s in new[] { -1, 1 })
            {
                var mb = New();
                mb.AddEllipsoid(new Point3D(0, 0, 0), headRx * 0.1, headRy * 0.52, headRx * 0.1, 14, 14);
                Add(mb, horn, EarTransform(s, rx: headRx * 0.1, ry: headRy * 0.52, hornX, hornBaseY, hornZ,
                    tiltZ: 16, tiltX: -34, push: 0));
            }
        }

        // ---- Arms --------------------------------------------------------
        {
            double shoulderY = torsoTopY - torsoRy * 0.18;
            double shoulderX = torsoRx * 0.95;
            double upperD = 0.13 * build;
            double foreD = 0.11 * build;

            var armMb = New();
            var pawMb = New();
            foreach (int s in new[] { -1, 1 })
            {
                var shoulder = new Point3D(s * shoulderX, shoulderY, 0);
                var elbow = new Point3D(s * (shoulderX + 0.07), shoulderY - 0.27 * limb, 0.03);
                var hand = new Point3D(s * (shoulderX + 0.02), shoulderY - 0.52 * limb, 0.07);

                armMb.AddSphere(shoulder, upperD * 0.62, 16, 14);
                armMb.AddCylinder(shoulder, elbow, upperD, 16);
                armMb.AddSphere(elbow, foreD * 0.62, 16, 14);
                armMb.AddCylinder(elbow, hand, foreD, 16);
                pawMb.AddSphere(hand, foreD * 0.8, 16, 14);
            }
            Add(armMb, fur);
            Add(pawMb, accent);
        }

        // ---- Legs + feet -------------------------------------------------
        {
            double hipX = torsoRx * 0.55;
            double thighD = 0.17 * build;
            double shinD = 0.13 * build;

            var legMb = New();
            var footMb = New();
            foreach (int s in new[] { -1, 1 })
            {
                var hip = new Point3D(s * hipX, hipY + 0.02, 0);
                var knee = new Point3D(s * hipX, hipY * 0.5, 0.05);
                var ankle = new Point3D(s * hipX, 0.08, 0.0);

                legMb.AddSphere(hip, thighD * 0.6, 16, 14);
                legMb.AddCylinder(hip, knee, thighD, 16);
                legMb.AddSphere(knee, shinD * 0.62, 16, 14);
                legMb.AddCylinder(knee, ankle, shinD, 16);
                footMb.AddEllipsoid(new Point3D(s * hipX, 0.05, 0.1), 0.1 * build, 0.06, 0.2, 18, 14);
            }
            Add(legMb, fur);
            Add(footMb, fur);
        }

        // ---- Tail (chain of fluff balls along a drooping curve) ----------
        {
            int n = 10;
            double len = 0.55 * p.TailLength;
            double fluff = p.TailFluff;
            double baseZ = -torsoRz * 0.85;
            double baseY = hipY + 0.06;

            var tailMb = New();
            var tipMb = New();
            for (int i = 0; i < n; i++)
            {
                double t = i / (double)(n - 1);
                double z = baseZ - t * len * 1.35;
                double y = baseY + (-0.55 * t + 0.4 * t * t) * len;          // droop then slight upward curl
                double r = (0.05 + 0.13 * Math.Sin((0.18 + 0.78 * t) * Math.PI)) * fluff * build;
                var center = new Point3D(0, y, z);
                if (t < 0.72)
                    tailMb.AddSphere(center, r, 16, 14);
                else
                    tipMb.AddSphere(center, r, 16, 14);
            }
            Add(tailMb, fur);
            Add(tipMb, accent);
        }

        group.Freeze();
        return new BuildResult(group, tris);
    }

    /// <summary>
    /// Positions an ear/horn built at the origin: lift it so its base sits at y=0, tilt outward
    /// (<paramref name="tiltZ"/>) and back/forward (<paramref name="tiltX"/>) about that base, then move
    /// it onto the head. <paramref name="push"/> nudges inner-ear pieces toward the front face.
    /// </summary>
    private static Transform3D EarTransform(int side, double rx, double ry,
        double earX, double earBaseY, double earZ, double tiltZ, double tiltX, double push)
    {
        var group = new Transform3DGroup();
        group.Children.Add(new TranslateTransform3D(0, ry, push));                                  // base to origin
        group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), side * tiltZ)));
        group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), tiltX)));
        group.Children.Add(new TranslateTransform3D(side * earX, earBaseY, earZ));                   // onto the head
        group.Freeze();
        return group;
    }

    // --- Materials --------------------------------------------------------

    /// <summary>Soft diffuse fur with a glossiness-controlled specular highlight.</summary>
    private static Material Fur(Color color, double gloss)
    {
        var diffuseBrush = new SolidColorBrush(color);
        diffuseBrush.Freeze();
        var grp = new MaterialGroup();
        grp.Children.Add(new DiffuseMaterial(diffuseBrush));

        byte s = (byte)(40 + gloss * 200);
        var specBrush = new SolidColorBrush(Color.FromRgb(s, s, s));
        specBrush.Freeze();
        grp.Children.Add(new SpecularMaterial(specBrush, 8 + gloss * 110));
        grp.Freeze();
        return grp;
    }

    /// <summary>Wet, high-shine material for eyes.</summary>
    private static Material Glossy(Color color)
    {
        var diffuseBrush = new SolidColorBrush(color);
        diffuseBrush.Freeze();
        var grp = new MaterialGroup();
        grp.Children.Add(new DiffuseMaterial(diffuseBrush));
        var spec = new SolidColorBrush(Colors.White);
        spec.Freeze();
        grp.Children.Add(new SpecularMaterial(spec, 90));
        grp.Freeze();
        return grp;
    }
}
