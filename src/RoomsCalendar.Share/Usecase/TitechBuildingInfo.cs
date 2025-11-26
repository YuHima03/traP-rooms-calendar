using ZLinq;

namespace RoomsCalendar.Share.Usecase
{
    public readonly struct TitechBuildingInfo
    {
        readonly int _p0;
        readonly int _p1;
        readonly int _p2;
        readonly int _p3;

        public string RawName { get; }

        public ReadOnlySpan<char> Name => RawName.AsSpan()[_p0.._p3];

        public ReadOnlySpan<char> Location => RawName.AsSpan()[_p0.._p1];

        public ReadOnlySpan<char> LocationAndIdNumber => RawName.AsSpan()[_p0.._p2];

        public int IdNumber => int.Parse(RawName.AsSpan()[_p1.._p2]);

        public ReadOnlySpan<char> IdSuffix => RawName.AsSpan()[_p2.._p3];

        public TitechBuildingInfo(string name)
        {
            RawName = name;

            _p0 = name.AsSpan()
                .AsValueEnumerable()
                .TakeWhile(char.IsWhiteSpace)
                .Count();
            _p1 = _p0 + name.AsSpan(_p0)
                .AsValueEnumerable()
                .TakeWhile(c => !char.IsDigit(c))
                .Count();
            _p2 = _p1 + name.AsSpan(_p1)
                .AsValueEnumerable()
                .TakeWhile(char.IsDigit)
                .Count();
            _p3 = name.Length - name.AsSpan()
                .AsValueEnumerable()
                .Reverse()
                .TakeWhile(char.IsWhiteSpace)
                .Count();
        }
    }
}
