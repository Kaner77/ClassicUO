﻿#region license

// Copyright (C) 2020 ClassicUO Development Community on Github
// 
// This project is an alternative client for the game Ultima Online.
// The goal of this is to develop a lightweight client considering
// new technologies.
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using ClassicUO.Network;

namespace ClassicUO.Game.Managers
{
    internal struct StepInfo
    {
        public byte Direction;
        public byte OldDirection;
        public byte Sequence;
        public bool Accepted;
        public bool Running;
        public bool NoRotation;
        public long Timer;
        public ushort X, Y;
        public sbyte Z;
    }

    internal class FastWalkStack
    {
        private readonly uint[] _keys = new uint[Constants.MAX_FAST_WALK_STACK_SIZE];

        public void SetValue(int index, uint value)
        {
            if (index >= 0 && index < Constants.MAX_FAST_WALK_STACK_SIZE)
            {
                _keys[index] = value;
            }
        }

        public void AddValue(uint value)
        {
            for (int i = 0; i < Constants.MAX_FAST_WALK_STACK_SIZE; i++)
            {
                if (_keys[i] == 0)
                {
                    _keys[i] = value;

                    break;
                }
            }
        }

        public uint GetValue()
        {
            for (int i = 0; i < Constants.MAX_FAST_WALK_STACK_SIZE; i++)
            {
                uint key = _keys[i];

                if (key != 0)
                {
                    _keys[i] = 0;

                    return key;
                }
            }

            return 0;
        }
    }

    internal class WalkerManager
    {
        public FastWalkStack FastWalkStack { get; } = new FastWalkStack();
        public ushort CurrentPlayerZ;
        public byte CurrentWalkSequence;
        public long LastStepRequestTime;
        public ushort NewPlayerZ;
        public bool ResendPacketResync;
        public StepInfo[] StepInfos = new StepInfo[Constants.MAX_STEP_COUNT]
        {
            new StepInfo(), new StepInfo(), new StepInfo(),
            new StepInfo(), new StepInfo()
        };
        public int StepsCount;
        public int UnacceptedPacketsCount;
        public bool WalkingFailed;
        public byte WalkSequence;
        public bool WantChangeCoordinates;

        public void DenyWalk(byte sequence, int x, int y, sbyte z)
        {
            World.Player.ClearSteps();

            Reset();

            if (x != -1)
            {
                World.Player.X = (ushort) x;
                World.Player.Y = (ushort) y;
                World.Player.Z = z;
                World.Player.UpdateScreenPosition();

                World.RangeSize.X = x;
                World.RangeSize.Y = y;

                World.Player.AddToTile();
            }
        }

        public void ConfirmWalk(byte sequence)
        {
            if (UnacceptedPacketsCount != 0)
            {
                UnacceptedPacketsCount--;
            }

            int stepIndex = 0;

            for (int i = 0; i < StepsCount; i++)
            {
                if (StepInfos[i].Sequence == sequence)
                {
                    break;
                }

                stepIndex++;
            }

            bool isBadStep = stepIndex == StepsCount;


            if (!isBadStep)
            {
                if (stepIndex >= CurrentWalkSequence)
                {
                    StepInfos[stepIndex].Accepted = true;

                    World.RangeSize.X = StepInfos[stepIndex].X;

                    World.RangeSize.Y = StepInfos[stepIndex].Y;
                }
                else if (stepIndex == 0)
                {
                    World.RangeSize.X = StepInfos[0].X;

                    World.RangeSize.Y = StepInfos[0].Y;

                    for (int i = 1; i < StepsCount; i++)
                    {
                        StepInfos[i - 1] = StepInfos[i];
                    }

                    StepsCount--;
                    CurrentWalkSequence--;
                }
                else
                {
                    isBadStep = true;
                }
            }

            if (isBadStep)
            {
                if (!ResendPacketResync)
                {
                    NetClient.Socket.Send(new PResend());
                    ResendPacketResync = true;
                }

                WalkingFailed = true;
                StepsCount = 0;
                CurrentWalkSequence = 0;
            }
        }

        public void Reset()
        {
            UnacceptedPacketsCount = 0;
            StepsCount = 0;
            WalkSequence = 0;
            CurrentWalkSequence = 0;
            WalkingFailed = false;
            ResendPacketResync = false;
            LastStepRequestTime = 0;
        }
    }
}