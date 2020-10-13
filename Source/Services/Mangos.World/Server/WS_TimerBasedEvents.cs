using System;
using System.Collections.Generic;
using System.Threading;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Player;
using Mangos.Common.Enums.Spell;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Spells;
using Mangos.World.Weather;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Server
{
	public class WS_TimerBasedEvents
	{
		public class TRegenerator : IDisposable
		{
			private Timer RegenerationTimer;

			private bool RegenerationWorking;

			private readonly int operationsCount;

			private int BaseMana;

			private int BaseLife;

			private int BaseRage;

			private int BaseEnergy;

			private bool _updateFlag;

			private bool NextGroupUpdate;

			public const int REGENERATION_TIMER = 2;

			public const int REGENERATION_ENERGY = 20;

			public const int REGENERATION_RAGE = 25;

			private bool _disposedValue;

			public TRegenerator()
			{
				RegenerationTimer = null;
				RegenerationWorking = false;
				NextGroupUpdate = true;
				RegenerationTimer = new Timer(new TimerCallback(Regenerate), null, 10000, 2000);
			}

			private void Regenerate(object state)
			{
				if (RegenerationWorking)
				{
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Update: Regenerator skipping update");
					return;
				}
				RegenerationWorking = true;
				NextGroupUpdate = !NextGroupUpdate;
				checked
				{
					try
					{
						WorldServiceLocator._WorldServer.CHARACTERs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
						foreach (KeyValuePair<ulong, WS_PlayerData.CharacterObject> Character in WorldServiceLocator._WorldServer.CHARACTERs)
						{
							if (Character.Value.DEAD || Character.Value.underWaterTimer != null || Character.Value.LogoutTimer != null || Character.Value.client == null)
							{
								continue;
							}
							WS_PlayerData.CharacterObject value = Character.Value;
							BaseMana = value.Mana.Current;
							BaseRage = value.Rage.Current;
							BaseEnergy = value.Energy.Current;
							BaseLife = value.Life.Current;
							_updateFlag = false;
							if (value.ManaType == ManaTypes.TYPE_RAGE)
							{
								if ((value.cUnitFlags & 0x80000) == 0)
								{
									if (value.Rage.Current > 0)
									{
										value.Rage.Current -= 25;
									}
								}
								else if (value.RageRegenBonus != 0)
								{
									value.Rage.Increment(value.RageRegenBonus);
								}
							}
							if (value.ManaType == ManaTypes.TYPE_ENERGY && value.Energy.Current < value.Energy.Maximum)
							{
								value.Energy.Increment(20);
							}
							if (value.ManaRegen == 0)
							{
								value.UpdateManaRegen();
							}
							if (value.spellCastManaRegeneration == 0)
							{
								if ((value.ManaType == ManaTypes.TYPE_MANA || value.Classe == Classes.CLASS_DRUID) && value.Mana.Current < value.Mana.Maximum)
								{
									value.Mana.Increment(value.ManaRegen * 2);
								}
							}
							else
							{
								if ((value.ManaType == ManaTypes.TYPE_MANA || value.Classe == Classes.CLASS_DRUID) && value.Mana.Current < value.Mana.Maximum)
								{
									value.Mana.Increment(value.ManaRegenInterrupt * 2);
								}
								if (value.spellCastManaRegeneration < 2)
								{
									value.spellCastManaRegeneration = 0;
								}
								else
								{
									value.spellCastManaRegeneration -= 2;
								}
							}
							if (value.Life.Current < value.Life.Maximum && (value.cUnitFlags & 0x80000) == 0)
							{
								switch (value.Classe)
								{
								case Classes.CLASS_MAGE:
									value.Life.Increment((int)Math.Round((double)value.Spirit.Base * 0.1 * (double)value.LifeRegenerationModifier) + value.LifeRegenBonus);
									break;
								case Classes.CLASS_PRIEST:
									value.Life.Increment((int)Math.Round((double)value.Spirit.Base * 0.1 * (double)value.LifeRegenerationModifier) + value.LifeRegenBonus);
									break;
								case Classes.CLASS_WARLOCK:
									value.Life.Increment((int)Math.Round((double)value.Spirit.Base * 0.11 * (double)value.LifeRegenerationModifier) + value.LifeRegenBonus);
									break;
								case Classes.CLASS_DRUID:
									value.Life.Increment((int)Math.Round((double)value.Spirit.Base * 0.11 * (double)value.LifeRegenerationModifier) + value.LifeRegenBonus);
									break;
								case Classes.CLASS_SHAMAN:
									value.Life.Increment((int)Math.Round((double)value.Spirit.Base * 0.11 * (double)value.LifeRegenerationModifier) + value.LifeRegenBonus);
									break;
								case Classes.CLASS_ROGUE:
									value.Life.Increment((int)Math.Round((double)value.Spirit.Base * 0.5 * (double)value.LifeRegenerationModifier) + value.LifeRegenBonus);
									break;
								case Classes.CLASS_WARRIOR:
									value.Life.Increment((int)Math.Round((double)value.Spirit.Base * 0.8 * (double)value.LifeRegenerationModifier) + value.LifeRegenBonus);
									break;
								case Classes.CLASS_HUNTER:
									value.Life.Increment((int)Math.Round((double)value.Spirit.Base * 0.25 * (double)value.LifeRegenerationModifier) + value.LifeRegenBonus);
									break;
								case Classes.CLASS_PALADIN:
									value.Life.Increment((int)Math.Round((double)value.Spirit.Base * 0.25 * (double)value.LifeRegenerationModifier) + value.LifeRegenBonus);
									break;
								}
							}
							if (BaseMana != value.Mana.Current)
							{
								_updateFlag = true;
								value.GroupUpdateFlag |= 16u;
								value.SetUpdateFlag(23, value.Mana.Current);
							}
							if ((BaseRage != value.Rage.Current) | ((value.cUnitFlags & 0x80000) == 524288))
							{
								_updateFlag = true;
								value.GroupUpdateFlag |= 16u;
								value.SetUpdateFlag(24, value.Rage.Current);
							}
							if (BaseEnergy != value.Energy.Current)
							{
								_updateFlag = true;
								value.GroupUpdateFlag |= 16u;
								value.SetUpdateFlag(26, value.Energy.Current);
							}
							if (BaseLife != value.Life.Current)
							{
								_updateFlag = true;
								value.SetUpdateFlag(22, value.Life.Current);
								value.GroupUpdateFlag |= 2u;
							}
							if (_updateFlag)
							{
								value.SendCharacterUpdate();
							}
							if (value.DuelOutOfBounds != 11)
							{
								value.DuelOutOfBounds -= 2;
								if (value.DuelOutOfBounds == 0)
								{
									WorldServiceLocator._WS_Spells.DuelComplete(ref value.DuelPartner, ref value.client.Character);
								}
							}
							value.CheckCombat();
							if (NextGroupUpdate)
							{
								value.GroupUpdate();
							}
							if (value.guidsForRemoving.Count > 0)
							{
								value.SendOutOfRangeUpdate();
							}
							value = null;
						}
						if (WorldServiceLocator._WorldServer.CHARACTERs_Lock.IsReaderLockHeld)
						{
							WorldServiceLocator._WorldServer.CHARACTERs_Lock.ReleaseReaderLock();
						}
					}
					catch (Exception ex2)
					{
						ProjectData.SetProjectError(ex2);
						Exception ex = ex2;
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Error at regenerate.{0}", Environment.NewLine + ex.ToString());
						ProjectData.ClearProjectError();
					}
					RegenerationWorking = false;
				}
			}

			protected virtual void Dispose(bool disposing)
			{
				if (!_disposedValue)
				{
					RegenerationTimer.Dispose();
					RegenerationTimer = null;
				}
				_disposedValue = true;
			}

			public void Dispose()
			{
				Dispose(disposing: true);
				GC.SuppressFinalize(this);
			}

			void IDisposable.Dispose()
			{
				//ILSpy generated this explicit interface implementation from .override directive in Dispose
				this.Dispose();
			}
		}

		public class TSpellManager : IDisposable
		{
			private Timer SpellManagerTimer;

			private bool SpellManagerWorking;

			public const int UPDATE_TIMER = 1000;

			private bool _disposedValue;

			public TSpellManager()
			{
				SpellManagerTimer = null;
				SpellManagerWorking = false;
				SpellManagerTimer = new Timer(new TimerCallback(Update), null, 10000, 1000);
			}

			private void Update(object state)
			{
				if (SpellManagerWorking)
				{
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Update: Spell Manager skipping update");
					return;
				}
				SpellManagerWorking = true;
				checked
				{
					try
					{
						WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
						long num = WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys.Count - 1;
						for (long i = 0L; i <= num; i++)
						{
							if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[Conversions.ToULong(WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys[(int)i])] != null)
							{
								Dictionary<ulong, WS_Creatures.CreatureObject> wORLD_CREATUREs;
								ulong key;
								WS_Base.BaseUnit objCharacter = (wORLD_CREATUREs = WorldServiceLocator._WorldServer.WORLD_CREATUREs)[key = Conversions.ToULong(WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys[(int)i])];
								UpdateSpells(ref objCharacter);
								wORLD_CREATUREs[key] = (WS_Creatures.CreatureObject)objCharacter;
							}
						}
					}
					catch (Exception ex4)
					{
						ProjectData.SetProjectError(ex4);
						Exception ex3 = ex4;
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, ex3.ToString(), null);
						ProjectData.ClearProjectError();
					}
					finally
					{
						if (WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.IsReaderLockHeld)
						{
							WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.ReleaseReaderLock();
						}
					}
					try
					{
						WorldServiceLocator._WorldServer.CHARACTERs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
						foreach (KeyValuePair<ulong, WS_PlayerData.CharacterObject> Character in WorldServiceLocator._WorldServer.CHARACTERs)
						{
							if (Character.Value != null)
							{
								WS_Base.BaseUnit objCharacter = Character.Value;
								UpdateSpells(ref objCharacter);
							}
						}
					}
					catch (Exception ex5)
					{
						ProjectData.SetProjectError(ex5);
						Exception ex2 = ex5;
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, ex2.ToString(), null);
						ProjectData.ClearProjectError();
					}
					finally
					{
						WorldServiceLocator._WorldServer.CHARACTERs_Lock.ReleaseLock();
					}
					List<WS_DynamicObjects.DynamicObjectObject> DynamicObjectsToDelete = new List<WS_DynamicObjects.DynamicObjectObject>();
					try
					{
						WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
						foreach (KeyValuePair<ulong, WS_DynamicObjects.DynamicObjectObject> Dynamic in WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs)
						{
							if (Dynamic.Value != null && Dynamic.Value.Update())
							{
								DynamicObjectsToDelete.Add(Dynamic.Value);
							}
						}
					}
					catch (Exception ex6)
					{
						ProjectData.SetProjectError(ex6);
						Exception ex = ex6;
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, ex.ToString(), null);
						ProjectData.ClearProjectError();
					}
					finally
					{
						WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs_Lock.ReleaseReaderLock();
					}
					foreach (WS_DynamicObjects.DynamicObjectObject item in DynamicObjectsToDelete)
					{
						item?.Delete();
					}
					SpellManagerWorking = false;
				}
			}

			protected virtual void Dispose(bool disposing)
			{
				if (!_disposedValue)
				{
					SpellManagerTimer.Dispose();
					SpellManagerTimer = null;
				}
				_disposedValue = true;
			}

			public void Dispose()
			{
				Dispose(disposing: true);
				GC.SuppressFinalize(this);
			}

			void IDisposable.Dispose()
			{
				//ILSpy generated this explicit interface implementation from .override directive in Dispose
				this.Dispose();
			}

			private void UpdateSpells(ref WS_Base.BaseUnit objCharacter)
			{
				if (objCharacter is WS_Totems.TotemObject)
				{
					((WS_Totems.TotemObject)objCharacter).Update();
					return;
				}
				checked
				{
					int num = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs - 1;
					for (int i = 0; i <= num; i++)
					{
						if (objCharacter.ActiveSpells[i] == null)
						{
							continue;
						}
						if (objCharacter.ActiveSpells[i].SpellDuration != WorldServiceLocator._Global_Constants.SPELL_DURATION_INFINITE)
						{
							objCharacter.ActiveSpells[i].SpellDuration -= 1000;
							byte j = 0;
							do
							{
								if (objCharacter.ActiveSpells[i] != null && objCharacter.ActiveSpells[i].Aura[j] != null && objCharacter.ActiveSpells[i].Aura_Info[j] != null && objCharacter.ActiveSpells[i].Aura_Info[j].Amplitude != 0 && unchecked(checked(objCharacter.ActiveSpells[i].GetSpellInfo.GetDuration - objCharacter.ActiveSpells[i].SpellDuration) % objCharacter.ActiveSpells[i].Aura_Info[j].Amplitude) == 0)
								{
									WS_Spells.ApplyAuraHandler obj = objCharacter.ActiveSpells[i].Aura[j];
									ref WS_Base.BaseUnit spellCaster = ref objCharacter.ActiveSpells[i].SpellCaster;
									ref WS_Base.BaseUnit reference = ref spellCaster;
									WS_Base.BaseObject Caster = spellCaster;
									obj(ref objCharacter, ref Caster, ref objCharacter.ActiveSpells[i].Aura_Info[j], objCharacter.ActiveSpells[i].SpellID, objCharacter.ActiveSpells[i].StackCount + 1, AuraAction.AURA_UPDATE);
									reference = (WS_Base.BaseUnit)Caster;
								}
								j = (byte)unchecked((uint)(j + 1));
							}
							while (unchecked((uint)j) <= 2u);
							if (objCharacter.ActiveSpells[i] != null && objCharacter.ActiveSpells[i].SpellDuration <= 0 && objCharacter.ActiveSpells[i].SpellDuration != WorldServiceLocator._Global_Constants.SPELL_DURATION_INFINITE)
							{
								objCharacter.RemoveAura(i, ref objCharacter.ActiveSpells[i].SpellCaster, RemovedByDuration: true);
							}
						}
						byte k = 0;
						do
						{
							if (objCharacter.ActiveSpells[i] != null && objCharacter.ActiveSpells[i].Aura_Info[k] != null && objCharacter.ActiveSpells[i].Aura_Info[k].ID == SpellEffects_Names.SPELL_EFFECT_APPLY_AREA_AURA)
							{
								if (objCharacter.ActiveSpells[i].SpellCaster == objCharacter)
								{
									List<WS_Base.BaseUnit> Targets = new List<WS_Base.BaseUnit>();
									if (objCharacter is WS_PlayerData.CharacterObject)
									{
										WS_Spells wS_Spells = WorldServiceLocator._WS_Spells;
										WS_PlayerData.CharacterObject objCharacter2 = (WS_PlayerData.CharacterObject)objCharacter;
										Targets = wS_Spells.GetPartyMembersAroundMe(ref objCharacter2, objCharacter.ActiveSpells[i].Aura_Info[k].GetRadius);
									}
									else if (objCharacter is WS_Totems.TotemObject && ((WS_Totems.TotemObject)objCharacter).Caster != null && ((WS_Totems.TotemObject)objCharacter).Caster is WS_PlayerData.CharacterObject)
									{
										WS_Spells wS_Spells2 = WorldServiceLocator._WS_Spells;
										ref WS_Base.BaseUnit caster2 = ref ((WS_Totems.TotemObject)objCharacter).Caster;
										ref WS_Base.BaseUnit reference = ref caster2;
										WS_PlayerData.CharacterObject objCharacter2 = (WS_PlayerData.CharacterObject)caster2;
										List<WS_Base.BaseUnit> partyMembersAtPoint = wS_Spells2.GetPartyMembersAtPoint(ref objCharacter2, objCharacter.ActiveSpells[i].Aura_Info[k].GetRadius, objCharacter.positionX, objCharacter.positionY, objCharacter.positionZ);
										reference = objCharacter2;
										Targets = partyMembersAtPoint;
									}
									foreach (WS_Base.BaseUnit item in Targets)
									{
										WS_Base.BaseUnit Unit = item;
										if (!Unit.HaveAura(objCharacter.ActiveSpells[i].SpellID))
										{
											WS_Spells wS_Spells3 = WorldServiceLocator._WS_Spells;
											WS_Base.BaseObject Caster = objCharacter;
											wS_Spells3.ApplyAura(ref Unit, ref Caster, ref objCharacter.ActiveSpells[i].Aura_Info[k], objCharacter.ActiveSpells[i].SpellID);
											objCharacter = (WS_Base.BaseUnit)Caster;
										}
									}
								}
								else if (objCharacter.ActiveSpells[i].SpellCaster != null && objCharacter.ActiveSpells[i].SpellCaster.Exist)
								{
									WS_PlayerData.CharacterObject caster = null;
									if (objCharacter.ActiveSpells[i].SpellCaster is WS_PlayerData.CharacterObject)
									{
										caster = (WS_PlayerData.CharacterObject)objCharacter.ActiveSpells[i].SpellCaster;
									}
									else if (objCharacter.ActiveSpells[i].SpellCaster is WS_Totems.TotemObject && ((WS_Totems.TotemObject)objCharacter.ActiveSpells[i].SpellCaster).Caster != null && ((WS_Totems.TotemObject)objCharacter.ActiveSpells[i].SpellCaster).Caster is WS_PlayerData.CharacterObject)
									{
										caster = (WS_PlayerData.CharacterObject)((WS_Totems.TotemObject)objCharacter.ActiveSpells[i].SpellCaster).Caster;
									}
									if (caster == null || caster.Group == null || !caster.Group.LocalMembers.Contains(objCharacter.GUID))
									{
										objCharacter.RemoveAura(i, ref objCharacter.ActiveSpells[i].SpellCaster);
									}
									else if (!objCharacter.ActiveSpells[i].SpellCaster.HaveAura(objCharacter.ActiveSpells[i].SpellID))
									{
										objCharacter.RemoveAura(i, ref objCharacter.ActiveSpells[i].SpellCaster);
									}
									else if (WorldServiceLocator._WS_Combat.GetDistance(objCharacter, objCharacter.ActiveSpells[i].SpellCaster) > objCharacter.ActiveSpells[i].Aura_Info[k].GetRadius)
									{
										objCharacter.RemoveAura(i, ref objCharacter.ActiveSpells[i].SpellCaster);
									}
								}
								else
								{
									objCharacter.RemoveAura(i, ref objCharacter.ActiveSpells[i].SpellCaster);
								}
							}
							k = (byte)unchecked((uint)(k + 1));
						}
						while (unchecked((uint)k) <= 2u);
					}
				}
			}
		}

		public class TAIManager : IDisposable
		{
			public Timer AIManagerTimer;

			private bool AIManagerWorking;

			public const int UPDATE_TIMER = 1000;

			private bool _disposedValue;

			public TAIManager()
			{
				AIManagerTimer = null;
				AIManagerWorking = false;
				AIManagerTimer = new Timer(new TimerCallback(Update), null, 10000, 1000);
			}

			private void Update(object state)
			{
				if (AIManagerWorking)
				{
					return;
				}
				int StartTime = WorldServiceLocator._NativeMethods.timeGetTime("");
				AIManagerWorking = true;
				try
				{
					WorldServiceLocator._WorldServer.WORLD_TRANSPORTs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
					foreach (KeyValuePair<ulong, WS_Transports.TransportObject> wORLD_TRANSPORT in WorldServiceLocator._WorldServer.WORLD_TRANSPORTs)
					{
						wORLD_TRANSPORT.Value.Update();
					}
				}
				catch (Exception ex5)
				{
					ProjectData.SetProjectError(ex5);
					Exception ex4 = ex5;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error updating transports.{0}{1}", Environment.NewLine, ex4.ToString());
					ProjectData.ClearProjectError();
				}
				finally
				{
					WorldServiceLocator._WorldServer.WORLD_TRANSPORTs_Lock.ReleaseReaderLock();
				}
				checked
				{
					try
					{
						WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
						try
						{
							long num = WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys.Count - 1;
							for (long i = 0L; i <= num; i++)
							{
								if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[Conversions.ToULong(WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys[(int)i])] != null && WorldServiceLocator._WorldServer.WORLD_CREATUREs[Conversions.ToULong(WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys[(int)i])].aiScript != null)
								{
									WorldServiceLocator._WorldServer.WORLD_CREATUREs[Conversions.ToULong(WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys[(int)i])].aiScript.DoThink();
								}
							}
						}
						catch (Exception ex6)
						{
							ProjectData.SetProjectError(ex6);
							Exception ex3 = ex6;
							WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error updating AI.{0}{1}", Environment.NewLine, ex3.ToString());
							ProjectData.ClearProjectError();
						}
						finally
						{
							WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.ReleaseReaderLock();
						}
					}
					catch (ApplicationException ex7)
					{
						ProjectData.SetProjectError(ex7);
						ApplicationException ex2 = ex7;
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Update: AI Manager timed out");
						ProjectData.ClearProjectError();
					}
					catch (Exception ex8)
					{
						ProjectData.SetProjectError(ex8);
						Exception ex = ex8;
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error updating AI.{0}{1}", Environment.NewLine, ex.ToString());
						ProjectData.ClearProjectError();
					}
					AIManagerWorking = false;
				}
			}

			protected virtual void Dispose(bool disposing)
			{
				if (!_disposedValue)
				{
					AIManagerTimer.Dispose();
					AIManagerTimer = null;
				}
				_disposedValue = true;
			}

			public void Dispose()
			{
				Dispose(disposing: true);
				GC.SuppressFinalize(this);
			}

			void IDisposable.Dispose()
			{
				//ILSpy generated this explicit interface implementation from .override directive in Dispose
				this.Dispose();
			}
		}

		public class TCharacterSaver : IDisposable
		{
			public Timer CharacterSaverTimer;

			private bool CharacterSaverWorking;

			public int UPDATE_TIMER;

			private bool _disposedValue;

			public TCharacterSaver()
			{
				CharacterSaverTimer = null;
				CharacterSaverWorking = false;
				UPDATE_TIMER = WorldServiceLocator._ConfigurationProvider.GetConfiguration().SaveTimer;
				CharacterSaverTimer = new Timer(new TimerCallback(Update), null, 10000, UPDATE_TIMER);
			}

			private void Update(object state)
			{
				if (CharacterSaverWorking)
				{
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Update: Character Saver skipping update");
					return;
				}
				CharacterSaverWorking = true;
				try
				{
					WorldServiceLocator._WorldServer.CHARACTERs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
					foreach (KeyValuePair<ulong, WS_PlayerData.CharacterObject> cHARACTER in WorldServiceLocator._WorldServer.CHARACTERs)
					{
						cHARACTER.Value.SaveCharacter();
					}
				}
				catch (Exception ex2)
				{
					ProjectData.SetProjectError(ex2);
					Exception ex = ex2;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, ex.ToString(), null);
					ProjectData.ClearProjectError();
				}
				finally
				{
					WorldServiceLocator._WorldServer.CHARACTERs_Lock.ReleaseReaderLock();
				}
				WorldServiceLocator._WS_Handlers_Instance.InstanceMapUpdate();
				CharacterSaverWorking = false;
			}

			protected virtual void Dispose(bool disposing)
			{
				if (!_disposedValue)
				{
					CharacterSaverTimer.Dispose();
					CharacterSaverTimer = null;
				}
				_disposedValue = true;
			}

			public void Dispose()
			{
				Dispose(disposing: true);
				GC.SuppressFinalize(this);
			}

			void IDisposable.Dispose()
			{
				//ILSpy generated this explicit interface implementation from .override directive in Dispose
				this.Dispose();
			}
		}

		public class TWeatherChanger : IDisposable
		{
			public Timer WeatherTimer;

			private bool WeatherWorking;

			public int UPDATE_TIMER;

			private bool _disposedValue;

			public TWeatherChanger()
			{
				WeatherTimer = null;
				WeatherWorking = false;
				UPDATE_TIMER = WorldServiceLocator._ConfigurationProvider.GetConfiguration().WeatherTimer;
				WeatherTimer = new Timer(new TimerCallback(Update), null, 10000, UPDATE_TIMER);
			}

			private void Update(object state)
			{
				if (WeatherWorking)
				{
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Update: Weather changer skipping update");
					return;
				}
				WeatherWorking = true;
				foreach (KeyValuePair<int, WS_Weather.WeatherZone> weatherZone in WorldServiceLocator._WS_Weather.WeatherZones)
				{
					weatherZone.Value.Update();
				}
				WeatherWorking = false;
			}

			protected virtual void Dispose(bool disposing)
			{
				if (!_disposedValue)
				{
					WeatherTimer.Dispose();
					WeatherTimer = null;
				}
				_disposedValue = true;
			}

			public void Dispose()
			{
				Dispose(disposing: true);
				GC.SuppressFinalize(this);
			}

			void IDisposable.Dispose()
			{
				//ILSpy generated this explicit interface implementation from .override directive in Dispose
				this.Dispose();
			}
		}

		public TRegenerator Regenerator;

		public TAIManager AIManager;

		public TSpellManager SpellManager;

		public TCharacterSaver CharacterSaver;

		public TWeatherChanger WeatherChanger;
	}
}
