﻿using System;

namespace Rubberduck.VBEditor.SafeComWrappers.VB.Abstract
{
    public interface IAddIn : ISafeComWrapper, IEquatable<IAddIn>
    {
        string ProgId { get; }
        string Guid { get; }
        string Description { get; set; }
        bool Connect { get; set; }
        object Object { get; set; }

        IVBE VBE { get; }
        IAddIns Collection { get; }
    }
}