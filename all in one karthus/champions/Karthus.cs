﻿using System;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Karthus
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        static float LastPingTime;

        public static void Load()
        {

            Q = new Spell(SpellSlot.Q, 875f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 1000f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 520f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(1f, 150f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(.25f, 1f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW(false);
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana(60);

            AIO_Menu.Champion.Lasthit.addUseQ();
            AIO_Menu.Champion.Lasthit.addIfMana(0);

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("Ping Notify on R killable enemies (local/client side)", true);

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();

            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            Q.MinHitChance = AIO_Menu.Champion.Misc.SelectedHitchance;

            if (Orbwalking.CanMove(100))
            {
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        Combo();
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        Harass();
                        break;
                    case Orbwalking.OrbwalkingMode.LastHit:
                        Lasthit();
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        Laneclear();
                        Jungleclear();
                        break;
                    case Orbwalking.OrbwalkingMode.None:
                        break;
                }
            }

            #region Ping Notify on R killable enemies
            if (R.IsReady() && AIO_Menu.Champion.Misc.getBoolValue("Ping Notify on R killable enemies (local/client side)"))
            {
                if (LastPingTime + 333 < Utils.GameTimeTickCount)
                {
                    foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget() && AIO_Func.isKillable(x, R)))
                        Game.ShowPing(PingCategory.Normal, target.Position, true);

                    LastPingTime = Utils.GameTimeTickCount;
                }
            }
            #endregion
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color, 3);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color, 3);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color, 3);
        }

        static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (!args.Unit.IsMe)
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit && Q.IsReady() && Player.ManaPercent > AIO_Menu.Champion.Lasthit.IfMana && AIO_Menu.Champion.Lasthit.UseQ))
                args.Process = false;
        }

        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
                Q.CastOnBestTarget(0f, false, true);

            if (AIO_Menu.Champion.Combo.UseW && W.IsReady())
                W.CastOnBestTarget(0f, false, true);

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
                if (AIO_Func.anyoneValidInRange(E.Range) && E.Instance.ToggleState == 1)
                    E.Cast();
                else if (!AIO_Func.anyoneValidInRange(E.Range) && E.Instance.ToggleState != 1)
                    E.Cast();
            }
        }

        static void Harass()
        {
            if (!(Player.ManaPercent > AIO_Menu.Champion.Harass.IfMana))
                return;

            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
                Q.CastOnBestTarget(0f, false, true);

            if (AIO_Menu.Champion.Harass.UseW && W.IsReady())
                W.CastOnBestTarget(0f, false, true);

            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            {
                if (AIO_Func.anyoneValidInRange(E.Range) && E.Instance.ToggleState == 1)
                    E.Cast();
                else if (!AIO_Func.anyoneValidInRange(E.Range) && E.Instance.ToggleState != 1)
                    E.Cast();
            }
        }

        static void Lasthit()
        {
            if (!(Player.ManaPercent > AIO_Menu.Champion.Lasthit.IfMana))
                return;

            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Lasthit.UseQ && Q.IsReady())
            {
                var qTarget = Minions.FirstOrDefault(x => x.IsValidTarget(Q.Range) && HealthPrediction.GetHealthPrediction((Obj_AI_Base)x, (int)(Q.Delay * 1000)) <= Q.GetDamage(x, 1));

                if (qTarget != null)
                    Q.Cast(qTarget);
            }
        }

        static void Laneclear()
        {
            if (!(Player.ManaPercent > AIO_Menu.Champion.Laneclear.IfMana))
                return;

            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            {
                var qloc = Q.GetCircularFarmLocation(Minions.Where(x => x.IsValidTarget(Q.Range)).ToList());

                if (qloc.MinionsHit >= 1)
                    Q.Cast(qloc.Position);
            }
        }

        static void Jungleclear()
        {
            if (!(Player.ManaPercent > AIO_Menu.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (AIO_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
            {
                var qloc = Q.GetCircularFarmLocation(Mobs.Where(x => x.IsValidTarget(Q.Range)).ToList());

                if (qloc.MinionsHit >= 1)
                    Q.Cast(qloc.Position);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage(enemy);

            if (E.IsReady())
                damage += E.GetDamage(enemy);

            if (R.IsReady())
                damage += R.GetDamage(enemy);

            return damage;
        }
    }
}

