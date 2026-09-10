using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using FluffiesEngine.Models;
using HelixToolkit.Wpf;

namespace FluffiesEngine.Rendering;

/// <summary>
/// Bespoke high-detail builder for <b>Vark</b>, the latex "Aeroket" (F-111 Aardvark theme):
/// a slender charcoal-latex anthro with a spiky olive ruff, huge half-lidded amber eyes,
/// swept ears with a red remove-before-flight flag, an antenna ahoge, a golden sensor dome,
/// military camo, star-roundel wing membranes, a banded tail and a "Tuna" bomb prop.
///
/// Everything is assembled from primitives at high subdivision so the silhouette reads as a
/// detailed, multi-polygon sculpt. Materials fake the wet-rubber look with a soft body sheen
/// plus a tight white highlight and a touch of self-illumination so the blacks keep their form.
/// </summary>
public static class VarkCharacterBuilder
{
    private static readonly ScaleTransform3D MirrorX = Freeze(new ScaleTransform3D(-1, 1, 1));

    public static BuildResult Build(CharacterParameters p)
    {
        var group = new Model3DGroup();
        int tris = 0;

        // ---------- Palette (driven by the colour pickers) ----------
        Color latexBase = p.FurPrimary;
        Color green = p.FurSecondary;
        Color greenDk = Mul(green, 0.62);
        Color greenLt = Mul(green, 1.18);
        Color camoBrown = Color.FromRgb(0x5C, 0x47, 0x31);
        Color camoDark = Mul(green, 0.42);
        Color amber = p.EyeColor;
        double gloss = p.Glossiness;

        // ---------- Materials ----------
        var latex = Latex(latexBase, gloss);
        var latexDk = Latex(Mul(latexBase, 0.72), gloss);
        var rubber = Rubber(green, gloss * 0.5 + 0.2);
        var rubberDk = Rubber(greenDk, gloss * 0.4 + 0.15);
        var rubberLt = Rubber(greenLt, gloss * 0.4 + 0.2);
        var brownMat = Rubber(camoBrown, 0.2);
        var camoMat = Rubber(camoDark, 0.25);
        var gold = Glossy(Color.FromRgb(0xF4, 0xB4, 0x3A), 0.18);
        var redMat = Rubber(Color.FromRgb(0xC9, 0x35, 0x2E), 0.35);
        var whiteMat = Flat(Color.FromRgb(0xEC, 0xEC, 0xE0));
        var blackMat = Latex(Color.FromRgb(0x12, 0x12, 0x15), Math.Max(gloss, 0.6));
        var orangeMat = Rubber(Color.FromRgb(0xE6, 0x88, 0x1E), 0.3);
        var eyeMat = EyeMaterial(amber);
        var blushMat = Translucent(Color.FromArgb(0x55, 0xE0, 0x74, 0x86));

        // Local helper: bake a mesh + material (+ optional transform) into the group, count tris.
        void Add(MeshBuilder mb, Material mat, Transform3D? t = null)
        {
            var mesh = mb.ToMesh();
            mesh.Freeze();
            tris += mesh.TriangleIndices.Count / 3;
            var m = new GeometryModel3D(mesh, mat) { BackMaterial = mat };
            if (t is not null) m.Transform = t;
            group.Children.Add(m);
        }
        MeshBuilder MB() => new(true, false);
        MeshBuilder UV() => new(true, true);

        // =================================================================
        //  Proportions  (slender anthro; whole model scaled by Height later)
        // =================================================================
        double build = p.Build;
        double legLen = 0.72;
        double hipY = legLen;
        double torsoH = 0.56;
        double torsoCenterY = hipY + torsoH * 0.5;
        double shoulderY = hipY + torsoH * 0.92;
        double torsoRx = 0.205 * build;
        double torsoRy = 0.34;
        double torsoRz = 0.155 * build;
        double neckY = shoulderY - 0.02;
        double headRx = 0.27, headRy = 0.275, headRz = 0.255;
        double headCenterY = neckY + 0.30;
        double headTopY = headCenterY + headRy;

        // =================================================================
        //  Contact shadow
        // =================================================================
        {
            var mb = MB();
            mb.AddEllipsoid(new Point3D(0, 0.004, 0.02), torsoRx * 2.4, 0.004, torsoRz * 3.4, 40, 6);
            Add(mb, Translucent(Color.FromArgb(0x66, 0, 0, 0)));
        }

        // =================================================================
        //  Wings / flight-membranes (built first so the body draws over them)
        // =================================================================
        var wingMat = Rubber(Mul(green, 0.5), gloss * 0.35 + 0.2);   // dark olive flight surface
        BuildWings(Add, wingMat, rubberLt, whiteMat, shoulderY, torsoRx, torsoRz);

        // =================================================================
        //  Torso, hips, chest
        // =================================================================
        {
            var mb = MB();
            mb.AddEllipsoid(new Point3D(0, hipY + 0.04, -0.005), torsoRx * 1.08, torsoRy * 0.52, torsoRz * 1.12, 44, 30);
            mb.AddEllipsoid(new Point3D(0, torsoCenterY + 0.06, 0.0), torsoRx, torsoRy, torsoRz, 48, 34);
            mb.AddEllipsoid(new Point3D(0, shoulderY - 0.05, 0.01), torsoRx * 0.96, torsoRy * 0.42, torsoRz * 0.98, 44, 28);
            Add(mb, latex);

            // Olive back + camo splotches
            var back = MB();
            back.AddEllipsoid(new Point3D(0, torsoCenterY + 0.07, -torsoRz * 0.55), torsoRx * 0.92, torsoRy * 0.86, torsoRz * 0.62, 40, 28);
            Add(back, rubber);

            var camo = MB();
            camo.AddEllipsoid(new Point3D(torsoRx * 0.5, torsoCenterY + 0.16, -torsoRz * 0.7), 0.07, 0.05, 0.03, 18, 14);
            camo.AddEllipsoid(new Point3D(-torsoRx * 0.45, torsoCenterY - 0.06, -torsoRz * 0.78), 0.06, 0.045, 0.03, 18, 14);
            Add(camo, brownMat);
        }

        // =================================================================
        //  Neck
        // =================================================================
        {
            var mb = MB();
            mb.AddCylinder(new Point3D(0, neckY - 0.05, 0), new Point3D(0, headCenterY - headRy * 0.55, 0.01), torsoRx * 0.9, 28);
            Add(mb, latex);
        }

        // =================================================================
        //  Head — dark latex face + olive crown/cheeks
        // =================================================================
        {
            var mb = MB();
            mb.AddEllipsoid(new Point3D(0, headCenterY, 0), headRx, headRy, headRz, 54, 40);
            // tiny muzzle bump
            mb.AddEllipsoid(new Point3D(0, headCenterY - headRy * 0.34, headRz * 0.66), headRx * 0.4, headRy * 0.24, 0.05, 26, 18);
            Add(mb, latex);

            // Olive crown + temples (offset up/back so the dark face stays in front)
            var cap = MB();
            cap.AddEllipsoid(new Point3D(0, headCenterY + 0.075, -0.03), headRx * 1.04, headRy * 0.82, headRz * 0.97, 50, 36);
            Add(cap, rubber);

            // little olive cheek pads
            var cheeks = MB();
            cheeks.AddEllipsoid(new Point3D(headRx * 0.82, headCenterY - headRy * 0.12, headRz * 0.42), 0.07, 0.10, 0.07, 24, 18);
            cheeks.AddEllipsoid(new Point3D(-headRx * 0.82, headCenterY - headRy * 0.12, headRz * 0.42), 0.07, 0.10, 0.07, 24, 18);
            Add(cheeks, rubber);
        }

        // =================================================================
        //  Eyes — big, half-lidded, glowing amber
        // =================================================================
        {
            double eyeY = headCenterY + headRy * 0.05;
            double eyeX = 0.116;
            double eyeZ = headRz * 0.80;

            foreach (int s in new[] { -1, 1 })
            {
                var yaw = Grp(Rot(0, 1, 0, s * 12), Tr(s * eyeX, eyeY, eyeZ));

                var ball = UV();
                ball.AddEllipsoid(new Point3D(0, 0, 0), 0.108, 0.10, 0.055, 40, 32);
                Add(ball, eyeMat, yaw);

                var pupil = MB();
                pupil.AddEllipsoid(new Point3D(0, -0.004, 0.052), 0.018, 0.052, 0.02, 18, 16);
                Add(pupil, blackMat, yaw);

                var hi = MB();
                hi.AddSphere(new Point3D(-s * 0.022, 0.035, 0.058), 0.02, 16, 14);
                Add(hi, whiteMat, yaw);

                // dark upper lid that drops over the top half -> sleepy look
                var lid = MB();
                lid.AddEllipsoid(new Point3D(0, 0.072, -0.004), 0.126, 0.10, 0.066, 36, 26);
                Add(lid, latex, yaw);

                // subtle lower lash
                var lash = MB();
                lash.AddEllipsoid(new Point3D(0, -0.082, 0.01), 0.11, 0.022, 0.05, 26, 14);
                Add(lash, latexDk, yaw);

                // blush
                var blush = MB();
                blush.AddEllipsoid(new Point3D(s * 0.13, eyeY - 0.085, headRz * 0.72), 0.05, 0.034, 0.02, 20, 14);
                Add(blush, blushMat);
            }

            // dark brow spikes
            foreach (int s in new[] { -1, 1 })
            {
                var spike = MB();
                AddCone(spike, 0.022, 0.095, 14);
                Add(spike, blackMat, Grp(Rot(0, 0, 1, s * 22), Rot(1, 0, 0, 26), Tr(s * 0.072, eyeY + 0.135, headRz * 0.72)));
            }

            // tiny nose + frown
            var nose = MB();
            nose.AddSphere(new Point3D(0, eyeY - 0.125, headRz * 0.93), 0.018, 16, 14);
            Add(nose, blackMat);
            var mouth = MB();
            mouth.AddEllipsoid(new Point3D(0, eyeY - 0.182, headRz * 0.86), 0.03, 0.011, 0.02, 18, 12);
            Add(mouth, blackMat);
        }

        // =================================================================
        //  Ears — big swept leaves (olive out, dark in), red flag on one
        // =================================================================
        {
            double earX = headRx * 0.72;
            double earY = headCenterY + headRy * 0.45;
            double earZ = -headRz * 0.05;
            double ear = p.EarSize;

            foreach (int s in new[] { -1, 1 })
            {
                // base of the leaf sits at origin; tilt out + a little back, then place on head
                var place = Grp(Rot(0, 0, 1, s * 46), Rot(1, 0, 0, -12), Tr(s * earX, earY, earZ));

                var outer = MB();
                outer.AddEllipsoid(new Point3D(0, 0.26 * ear, 0), 0.11 * ear, 0.30 * ear, 0.045, 30, 24);
                Add(outer, rubber, place);

                var inner = MB();
                inner.AddEllipsoid(new Point3D(0, 0.25 * ear, 0.035), 0.066 * ear, 0.22 * ear, 0.03, 26, 20);
                Add(inner, latexDk, place);

                var rim = MB();
                rim.AddEllipsoid(new Point3D(0, 0.40 * ear, 0.0), 0.055 * ear, 0.07 * ear, 0.05, 22, 16);
                Add(rim, rubberDk, place);
            }

            // red "remove before flight" flag on the right ear
            var flagPlace = Grp(Rot(0, 0, 1, -46), Rot(1, 0, 0, -12), Tr(earX, earY, earZ));
            var flag = MB();
            flag.AddEllipsoid(new Point3D(0.02, 0.12 * ear, 0.05), 0.05, 0.035, 0.012, 16, 12);
            Add(flag, redMat, flagPlace);
        }

        // =================================================================
        //  Spiky olive ruff (collar) + messy hair on top
        // =================================================================
        {
            // Ruff: a fan of spikes around the back/sides of the lower head
            double ringY = headCenterY - headRy * 0.45;
            double ringR = headRx * 0.95;
            int ruff = 16;
            for (int i = 0; i < ruff; i++)
            {
                double a = -150 + 300.0 * i / (ruff - 1);          // skip the face front
                double rad = a * Math.PI / 180.0;
                double x = Math.Sin(rad) * ringR;
                double z = -Math.Cos(rad) * ringR * 0.9 - 0.02;
                double len = 0.16 + 0.07 * Math.Cos(rad);          // longer at the back
                var spike = MB();
                AddCone(spike, 0.05, len, 12);
                var place = Grp(
                    Rot(1, 0, 0, -55),                              // lay it outward/back
                    Rot(0, 1, 0, a),                                // aim around the ring
                    Tr(x, ringY, z));
                Add(spike, i % 3 == 0 ? rubberDk : rubber, place);
            }

            // Bigger flat cheek plates (signature leaf ruff)
            foreach (int s in new[] { -1, 1 })
            {
                for (int k = 0; k < 3; k++)
                {
                    double tilt = 30 + k * 26;
                    var place = Grp(Rot(0, 0, 1, s * tilt), Rot(1, 0, 0, -28), Tr(s * headRx * 0.85, headCenterY - headRy * 0.2 - k * 0.02, -0.04));
                    var plate = MB();
                    plate.AddEllipsoid(new Point3D(0, 0.13, 0), 0.085, 0.16, 0.02, 24, 18);
                    Add(plate, rubber, place);
                    var inlay = MB();
                    inlay.AddEllipsoid(new Point3D(0, 0.14, 0.018), 0.045, 0.10, 0.012, 20, 14);
                    Add(inlay, rubberDk, place);
                }
            }

            // Top hair spikes
            int hair = 7;
            for (int i = 0; i < hair; i++)
            {
                double t = i / (double)(hair - 1);
                double x = (t - 0.5) * headRx * 1.3;
                double len = 0.18 + 0.10 * Math.Sin(t * Math.PI);
                var spike = MB();
                AddCone(spike, 0.045, len, 12);
                var place = Grp(Rot(1, 0, 0, -18 - 12 * Math.Sin(t * Math.PI)), Rot(0, 0, 1, (t - 0.5) * 36), Tr(x, headTopY - 0.04, -0.02));
                Add(spike, i % 2 == 0 ? rubber : rubberDk, place);
            }

            // Antenna ahoge — a tapered sprig that arcs up and forward, then hooks
            var ant = MB();
            int na = 16;
            for (int i = 0; i <= na; i++)
            {
                double t = i / (double)na;
                double arc = t * 1.7;                                  // sweeps forward as it rises
                double x = 0.015 * Math.Sin(t * Math.PI);
                double y = headTopY + 0.02 + 0.19 * Math.Sin(arc);
                double z = headRz * 0.18 + 0.16 * (1 - Math.Cos(arc));  // curls toward the front
                double r = 0.03 * (1 - 0.72 * t) + 0.006;
                ant.AddSphere(new Point3D(x, y, z), r, 16, 12);
            }
            Add(ant, rubber);
        }

        // =================================================================
        //  Golden sensor dome on the upper-right forehead / hairline
        // =================================================================
        {
            var domeCenter = new Point3D(0.11, headCenterY + 0.14, 0.205);
            var baseRing = MB();
            baseRing.AddEllipsoid(new Point3D(0.11, headCenterY + 0.14, 0.185), 0.055, 0.052, 0.03, 24, 16);
            Add(baseRing, latexDk);
            var dome = MB();
            dome.AddSphere(domeCenter, 0.056, 30, 24);
            Add(dome, gold);
            // little highlight so it reads as a glossy canopy
            var glint = MB();
            glint.AddSphere(new Point3D(domeCenter.X - 0.018, domeCenter.Y + 0.018, domeCenter.Z + 0.04), 0.012, 12, 10);
            Add(glint, whiteMat);
        }

        // =================================================================
        //  Arms (relaxed at the sides) — dark latex with olive cuffs
        // =================================================================
        {
            double shX = torsoRx * 1.04;
            foreach (int s in new[] { -1, 1 })
            {
                var shoulder = new Point3D(s * shX, shoulderY, 0);
                var elbow = new Point3D(s * (shX + 0.05), shoulderY - 0.27, 0.04);
                var hand = new Point3D(s * (shX + 0.015), shoulderY - 0.52, 0.11);

                var arm = MB();
                arm.AddSphere(shoulder, 0.085, 24, 20);
                arm.AddCylinder(shoulder, elbow, 0.13, 22);
                arm.AddSphere(elbow, 0.062, 22, 18);
                arm.AddCylinder(elbow, hand, 0.10, 22);
                Add(arm, latex);

                // olive wrist cuff
                var cuff = MB();
                cuff.AddEllipsoid(new Point3D((hand.X + elbow.X) * 0.5 + s * 0.005, (hand.Y + elbow.Y) * 0.5, (hand.Z + elbow.Z) * 0.5), 0.062, 0.06, 0.062, 22, 16);
                Add(cuff, rubber);

                // paw + finger nubs
                var paw = MB();
                paw.AddSphere(hand, 0.075, 24, 20);
                Add(paw, latex);
                var fingers = MB();
                for (int f = 0; f < 3; f++)
                {
                    double off = (f - 1) * 0.035;
                    fingers.AddSphere(new Point3D(hand.X + off, hand.Y - 0.06, hand.Z + 0.03), 0.026, 14, 12);
                }
                Add(fingers, latex);

                // dark camo patch on the upper arm
                if (s == 1)
                {
                    var patch = MB();
                    patch.AddEllipsoid(new Point3D(s * (shX + 0.02), shoulderY - 0.14, -0.05), 0.05, 0.07, 0.04, 18, 14);
                    Add(patch, camoMat);
                }
            }
        }

        // =================================================================
        //  Legs + digitigrade-ish feet — star roundel on the right thigh
        // =================================================================
        {
            double hipX = torsoRx * 0.62;
            foreach (int s in new[] { -1, 1 })
            {
                var hip = new Point3D(s * hipX, hipY + 0.02, 0);
                var knee = new Point3D(s * hipX, hipY * 0.5, 0.06);
                var ankle = new Point3D(s * hipX, 0.10, -0.01);

                var leg = MB();
                leg.AddSphere(hip, 0.115, 24, 20);
                leg.AddCylinder(hip, knee, 0.175, 24);
                leg.AddSphere(knee, 0.082, 22, 18);
                leg.AddCylinder(knee, ankle, 0.13, 22);
                Add(leg, latex);

                // olive shin guard
                var shin = MB();
                shin.AddEllipsoid(new Point3D(s * hipX, hipY * 0.32, 0.085), 0.06, 0.11, 0.05, 22, 16);
                Add(shin, rubber);

                // foot (dark) + olive toes
                var foot = MB();
                foot.AddEllipsoid(new Point3D(s * hipX, 0.055, 0.11), 0.092, 0.058, 0.20, 26, 18);
                Add(foot, latex);
                var toes = MB();
                for (int t = 0; t < 3; t++)
                {
                    double off = (t - 1) * 0.05;
                    toes.AddEllipsoid(new Point3D(s * hipX + off, 0.045, 0.27), 0.03, 0.03, 0.05, 16, 12);
                }
                Add(toes, rubber);
            }

            // green oval + white star roundel on the right (+x) outer thigh
            var thighPoint = new Point3D(hipX + 0.02, hipY - 0.14, 0.02);
            var patchPlace = Grp(Rot(0, 1, 0, 90), Tr(thighPoint.X + 0.16, thighPoint.Y, thighPoint.Z));
            var oval = MB();
            oval.AddEllipsoid(new Point3D(0, 0, 0), 0.075, 0.075, 0.012, 28, 8);
            Add(oval, rubber, patchPlace);
            var star = MB();
            AddStar(star, 0.058, 0.024, 0.004);
            Add(star, whiteMat, Grp(Rot(0, 1, 0, 90), Tr(thighPoint.X + 0.172, thighPoint.Y, thighPoint.Z)));
        }

        // =================================================================
        //  Tail — long, banded olive base fading to dark latex tip
        // =================================================================
        {
            double fluff = p.TailFluff;
            double len = p.TailLength;
            int n = 16;
            var olive = MB();
            var band = MB();
            var dark = MB();
            for (int i = 0; i < n; i++)
            {
                double t = i / (double)(n - 1);
                double z = -torsoRz * 0.8 - t * 0.95 * len;
                double x = -Math.Sin(t * 1.4) * 0.18;                 // sweep to the left
                double y = hipY + 0.08 - t * 0.55 + 0.22 * t * t;     // droop then settle
                double r = (0.115 - 0.085 * t) * (0.7 + 0.3 * fluff);
                var c = new Point3D(x, Math.Max(0.16, y), z);
                if (t < 0.2) olive.AddSphere(c, r, 22, 18);
                else if (t < 0.27) band.AddSphere(c, r * 1.04, 22, 18);
                else dark.AddSphere(c, r, 22, 18);
            }
            Add(olive, rubber);
            Add(band, rubberDk);
            Add(dark, latex);

            // brown camo splotch + a couple of dark patches on the tail
            var camo = MB();
            camo.AddSphere(new Point3D(-0.05, hipY - 0.02, -torsoRz * 0.8 - 0.16), 0.06, 18, 14);
            Add(camo, brownMat);
        }

        // =================================================================
        //  "Tuna" bomb prop, leaning nose-down beside Vark
        // =================================================================
        BuildBomb(Add, rubber, rubberDk, orangeMat, whiteMat, latexDk);

        // ---------- Finalise ----------
        double scale = p.Height;
        group.Transform = Freeze(new ScaleTransform3D(scale, scale, scale));
        return new BuildResult(group, tris);
    }

    // =====================================================================
    //  Sub-assemblies
    // =====================================================================

    private static void BuildWings(Action<MeshBuilder, Material, Transform3D?> add,
        Material dark, Material edge, Material star, double shoulderY, double torsoRx, double torsoRz)
    {
        const int nSpan = 7;
        var leading = new Point3D[nSpan + 1];
        var leadInner = new Point3D[nSpan + 1];
        var trailing = new Point3D[nSpan + 1];
        for (int s = 0; s <= nSpan; s++)
        {
            double fs = s / (double)nSpan;
            double chord = Lerp(0.52, 0.18, fs);
            double bx = 0.05 + fs * 0.98 + Math.Sin(fs * Math.PI) * 0.06;
            double by = -fs * 1.18;
            double bz = -0.12 - fs * 0.78;
            leading[s] = new Point3D(bx, by, bz + chord * 0.5);
            leadInner[s] = new Point3D(bx, by, bz + chord * 0.5 - 0.07);
            trailing[s] = new Point3D(bx, by, bz - chord * 0.5);
        }

        var darkMb = new MeshBuilder(true, false);
        var edgeMb = new MeshBuilder(true, false);
        for (int s = 0; s < nSpan; s++)
        {
            edgeMb.AddQuad(leading[s], leading[s + 1], leadInner[s + 1], leadInner[s]);
            darkMb.AddQuad(leadInner[s], leadInner[s + 1], trailing[s + 1], trailing[s]);
        }

        var starMb = new MeshBuilder(true, false);
        AddStar(starMb, 0.12, 0.05, 0.006);

        var rootT = Grp(Tr(torsoRx * 0.8, shoulderY - 0.02, -torsoRz * 0.55));
        var starT = Grp(Rot(0, 1, 0, 18), Tr(0.55, shoulderY - 0.72, -0.95));

        // right side
        add(darkMb, dark, rootT);
        add(edgeMb, edge, rootT);
        add(starMb, star, starT);
        // left side = the same meshes mirrored across X = 0 (re-baked with a mirror transform)
        add(darkMb, dark, Grp(rootT, MirrorX));
        add(edgeMb, edge, Grp(rootT, MirrorX));
        add(starMb, star, Grp(starT, MirrorX));
    }

    private static void BuildBomb(Action<MeshBuilder, Material, Transform3D?> add,
        Material body, Material fin, Material orange, Material white, Material dark)
    {
        // Local frame: axis +Y, nose at the bottom (y≈0), fins at the top.
        var place = Grp(Rot(0, 0, 1, -13), Tr(-0.62, 0.05, 0.32));

        var shadow = new MeshBuilder(true, false);
        shadow.AddEllipsoid(new Point3D(-0.5, 0.004, 0.34), 0.12, 0.004, 0.16, 24, 6);
        add(shadow, Translucent(Color.FromArgb(0x55, 0, 0, 0)), null);

        var bodyMb = new MeshBuilder(true, false);
        bodyMb.AddCylinder(new Point3D(0, 0.12, 0), new Point3D(0, 0.66, 0), 0.135, 28);
        bodyMb.AddEllipsoid(new Point3D(0, 0.1, 0), 0.067, 0.13, 0.067, 28, 20);   // ogive nose
        bodyMb.AddEllipsoid(new Point3D(0, 0.66, 0), 0.067, 0.05, 0.067, 28, 16);  // tail cap
        add(bodyMb, body, place);

        var bandMb = new MeshBuilder(true, false);
        bandMb.AddCylinder(new Point3D(0, 0.5, 0), new Point3D(0, 0.56, 0), 0.142, 28);
        add(bandMb, orange, place);

        var noseTip = new MeshBuilder(true, false);
        noseTip.AddSphere(new Point3D(0, 0.04, 0), 0.03, 18, 14);
        add(noseTip, white, place);

        // four cruciform fins near the tail
        for (int f = 0; f < 4; f++)
        {
            var finMb = new MeshBuilder(true, false);
            finMb.AddQuad(
                new Point3D(0.0, 0.56, 0),
                new Point3D(0.0, 0.73, 0),
                new Point3D(0.17, 0.73, 0),
                new Point3D(0.19, 0.6, 0));
            add(finMb, fin, Grp(Rot(0, 1, 0, f * 90), place));
        }
    }

    // =====================================================================
    //  Geometry helpers (all build into a MeshBuilder; triangles counted by Add)
    // =====================================================================

    /// <summary>Solid cone, base circle in the XZ plane at the origin, apex toward +Y.</summary>
    private static void AddCone(MeshBuilder mb, double baseR, double height, int seg)
    {
        var apex = new Point3D(0, height, 0);
        var center = new Point3D(0, 0, 0);
        for (int i = 0; i < seg; i++)
        {
            double a0 = 2 * Math.PI * i / seg, a1 = 2 * Math.PI * (i + 1) / seg;
            var p0 = new Point3D(baseR * Math.Cos(a0), 0, baseR * Math.Sin(a0));
            var p1 = new Point3D(baseR * Math.Cos(a1), 0, baseR * Math.Sin(a1));
            mb.AddTriangle(apex, p0, p1);
            mb.AddTriangle(center, p1, p0);
        }
    }

    /// <summary>Flat 5-point star in the XY plane (facing +Z), centred at the origin.</summary>
    private static void AddStar(MeshBuilder mb, double outerR, double innerR, double z)
    {
        var center = new Point3D(0, 0, z);
        var pts = new Point3D[10];
        for (int i = 0; i < 10; i++)
        {
            double r = (i % 2 == 0) ? outerR : innerR;
            double a = Math.PI / 2 + i * Math.PI / 5;   // point up
            pts[i] = new Point3D(r * Math.Cos(a), r * Math.Sin(a), z);
        }
        for (int i = 0; i < 10; i++)
            mb.AddTriangle(center, pts[i], pts[(i + 1) % 10]);
    }

    // =====================================================================
    //  Materials
    // =====================================================================

    private static Material Latex(Color c, double gloss)
    {
        var g = new MaterialGroup();
        g.Children.Add(new DiffuseMaterial(FrozenBrush(c)));
        g.Children.Add(new SpecularMaterial(FrozenBrush(Color.FromRgb(58, 58, 64)), 11));     // broad body sheen
        byte hi = (byte)(140 + gloss * 115);
        g.Children.Add(new SpecularMaterial(FrozenBrush(Color.FromRgb(hi, hi, hi)), 58 + gloss * 85)); // tight wet highlight
        g.Children.Add(new EmissiveMaterial(FrozenBrush(Mul(c, 0.16))));                       // keep blacks readable
        g.Freeze();
        return g;
    }

    private static Material Rubber(Color c, double gloss)
    {
        var g = new MaterialGroup();
        g.Children.Add(new DiffuseMaterial(FrozenBrush(c)));
        byte hi = (byte)(70 + gloss * 120);
        g.Children.Add(new SpecularMaterial(FrozenBrush(Color.FromRgb(hi, hi, hi)), 18 + gloss * 55));
        g.Children.Add(new EmissiveMaterial(FrozenBrush(Mul(c, 0.1))));
        g.Freeze();
        return g;
    }

    private static Material Glossy(Color c, double emissive)
    {
        var g = new MaterialGroup();
        g.Children.Add(new DiffuseMaterial(FrozenBrush(c)));
        g.Children.Add(new SpecularMaterial(FrozenBrush(Colors.White), 95));
        g.Children.Add(new EmissiveMaterial(FrozenBrush(Mul(c, emissive))));
        g.Freeze();
        return g;
    }

    private static Material Flat(Color c)
    {
        var m = new DiffuseMaterial(FrozenBrush(c));
        m.Freeze();
        return m;
    }

    private static Material Translucent(Color argb)
    {
        var b = new SolidColorBrush(argb);
        b.Freeze();
        var m = new DiffuseMaterial(b);
        m.Freeze();
        return m;
    }

    /// <summary>Amber eye: a vertical top→bottom gradient (mapped via UVs) plus a glow and wet highlight.</summary>
    private static Material EyeMaterial(Color amber)
    {
        var grad = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(0, 1) };
        grad.GradientStops.Add(new GradientStop(Mul(amber, 1.32), 0.0));
        grad.GradientStops.Add(new GradientStop(amber, 0.5));
        grad.GradientStops.Add(new GradientStop(Mul(amber, 0.62), 1.0));
        grad.Freeze();

        var g = new MaterialGroup();
        g.Children.Add(new DiffuseMaterial(grad));
        g.Children.Add(new EmissiveMaterial(FrozenBrush(Mul(amber, 0.42))));
        g.Children.Add(new SpecularMaterial(FrozenBrush(Colors.White), 85));
        g.Freeze();
        return g;
    }

    // =====================================================================
    //  Small utilities
    // =====================================================================

    private static SolidColorBrush FrozenBrush(Color c)
    {
        var b = new SolidColorBrush(c);
        b.Freeze();
        return b;
    }

    private static byte Cl(double v) => (byte)Math.Max(0, Math.Min(255, Math.Round(v)));
    private static Color Mul(Color c, double f) => Color.FromRgb(Cl(c.R * f), Cl(c.G * f), Cl(c.B * f));
    private static double Lerp(double a, double b, double t) => a + (b - a) * t;

    private static TranslateTransform3D Tr(double x, double y, double z) => Freeze(new TranslateTransform3D(x, y, z));
    private static RotateTransform3D Rot(double ax, double ay, double az, double deg)
        => Freeze(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(ax, ay, az), deg)));

    private static Transform3D Grp(params Transform3D[] parts)
    {
        var g = new Transform3DGroup();
        foreach (var t in parts) g.Children.Add(t);
        g.Freeze();
        return g;
    }

    private static T Freeze<T>(T f) where T : Freezable
    {
        f.Freeze();
        return f;
    }
}
