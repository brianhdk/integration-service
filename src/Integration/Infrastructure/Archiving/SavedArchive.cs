﻿using System;

namespace Vertica.Integration.Infrastructure.Archiving
{
    public class SavedArchive
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ByteSize { get; set; }
        public DateTimeOffset Created { get; set; }
    }
}