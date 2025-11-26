using ZLinq;

namespace RoomsCalendar.Share.Usecase
{
    public readonly struct TitechRoomInfo
    {
        readonly int _p0;
        readonly int _p1;
        readonly int _p2;
        readonly int _p3;
        readonly int _p4;

        public string RawName { get; }

        public ReadOnlySpan<char> Name => RawName.AsSpan()[_p0.._p2];

        public ReadOnlySpan<char> OldName => RawName.AsSpan()[(_p3 + 1)..(_p4 - 1)];

        public TitechBuildingInfo Building { get; }

        public ReadOnlySpan<char> RoomNumber => RawName.AsSpan()[(_p1 + 1).._p2];

        public int? Capacity { get; }

        public TitechRoomInfo(string name, int? capacity = null)
        {
            RawName = name;
            Capacity = capacity;

            _p0 = name.AsSpan()
                .AsValueEnumerable()
                .TakeWhile(char.IsWhiteSpace)
                .Count();
            _p1 = name.AsSpan().IndexOf('-');
            _p2 = _p1 + 1 + name.AsSpan(_p1 + 1)
                .AsValueEnumerable()
                .TakeWhile(char.IsLetterOrDigit)
                .Count();
            _p3 = _p2 + name.AsSpan(_p2)
                .AsValueEnumerable()
                .TakeWhile(char.IsWhiteSpace)
                .Count();
            _p4 = name.Length - name.AsSpan()
                .AsValueEnumerable()
                .Reverse()
                .TakeWhile(char.IsWhiteSpace)
                .Count();

            Building = new(string.Intern(name[_p0.._p1]));
        }
    }
}
