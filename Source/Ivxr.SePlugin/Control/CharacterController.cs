﻿using Iv4xr.SePlugin.WorldModel;
using Sandbox.Game.Entities;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Text;
using VRageMath;

namespace Iv4xr.SePlugin.Control
{
	public interface ICharacterController
	{
		void Move(Vector3 move, Vector2 rotation, float roll);
		void Move(MoveAndRotateArgs args);
		void Interact(InteractionArgs args);
	}

	public class CharacterController : ICharacterController
	{
		private IGameSession m_session;

		public CharacterController(IGameSession session)
		{
			m_session = session;
		}

		public void Move(MoveAndRotateArgs args)
		{
			Move(args.Movement, args.Rotation, (float)args.Roll);
		}

		public void Move(Vector3 movement, Vector2 rotation, float roll)
		{
			var entityController = GetEntityController();

			entityController.ControlledEntity.MoveAndRotate(movement, rotation, roll);
		}

		public void Interact(InteractionArgs args)
		{
			if (args.InteractionType == InteractionType.EQUIP)
			{
				EquipToolbarItem(args.Slot, args.Page);
			}
			else if (args.InteractionType == InteractionType.PLACE)
			{
				PlaceItem();
			}
			else
			{
				throw new ArgumentException("Unknown or not implemented interaction type.");
			}
		}

		private void PlaceItem()
		{
			// copied from MyCubePlacer.Shoot
			bool isAdminOrCreative =
				(MySession.Static.CreativeToolsEnabled(Sync.MyId) && MySession.Static.HasCreativeRights)
				|| MySession.Static.CreativeMode;

			if (isAdminOrCreative)
			{
				if (MyCubeBuilder.Static is null)
					throw new NullReferenceException("Cube builder is null.");

				MyCubeBuilder.Static.Add();
				return;
			}

			// else: use tool's primary action in survival mode
			var entityController = GetEntityController();
			entityController.ControlledEntity.BeginShoot(MyShootActionEnum.PrimaryAction);
		}

		private void EquipToolbarItem(int slot, int page)
		{
			var currentToolbar = MyToolbarComponent.CurrentToolbar;

			if (page >= 0)
				currentToolbar.SwitchToPage(page);

			currentToolbar.ActivateItemAtSlot(slot);
		}

		private MyEntityController GetEntityController()
		{
			if (m_session.Character is null)
				throw new NullReferenceException("I'm out of character!");  // Should not happen.

			var entityController = m_session.Character.ControllerInfo.Controller;

			if (entityController is null)  // Happens when the character enters a vehicle, for example.
				throw new NotSupportedException("Entity control not possible now.");

			return entityController;
		}
	}
}
