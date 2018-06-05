/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NosSharp.Enums;

namespace OpenNos.DAL.EF.Entities
{
    public class UpgradeLog
    {
        #region Instantiation

        #endregion

        #region Properties

        [Key]
        public long Id { get; set; }

        public long AccountId { get; set; }

        public long CharacterId { get; set; }

        public string CharacterName { get; set; }

        public string UpgradeType { get; set; }

        public bool? HasAmulet { get; set; }

        public DateTime Date { get; set; }

        public bool Success { get; set; }

        public short ItemVnum { get; set; }

        public string ItemName { get; set; }

        #endregion
    }
}