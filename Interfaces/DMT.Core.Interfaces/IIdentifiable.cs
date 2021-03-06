﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Interfaces.Serialization;

namespace DMT.Core.Interfaces
{
    /// <summary>
    /// Describes an identifiable entity. It has only an id.
    /// </summary>
    public interface IIdentifiable : ISerializable
    {
        /// <summary>
        /// Gets the unique identifier for the entity.
        /// </summary>
        IId Id { get; }
    }
}
