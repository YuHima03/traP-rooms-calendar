using RoomsCalendar.Share.Usecase;
using System.Collections.Frozen;

namespace RoomsCalendar.Share.Constants
{
    public static class TitechRooms
    {
        public static readonly FrozenDictionary<string, TitechRoomInfo> Rooms = FrozenDictionary.ToFrozenDictionary(
            [
                // M-B
                new TitechRoomInfo("M-B07 (H101)", 200),
                new TitechRoomInfo("M-B101 (H102)", 48),
                new TitechRoomInfo("M-B104 (H103)", 96),
                new TitechRoomInfo("M-B107 (H104)", 48),
                new TitechRoomInfo("M-B45 (H105)", 54),
                new TitechRoomInfo("M-B43 (H106)", 54),
                // M-1
                new TitechRoomInfo("M-123 (H111)", 112),
                new TitechRoomInfo("M-110 (H112)", 80),
                new TitechRoomInfo("M-107 (H113)", 75),
                new TitechRoomInfo("M-103 (H114)", 98),
                new TitechRoomInfo("M-102 (H115)", 42),
                new TitechRoomInfo("M-101 (H116)", 72),
                new TitechRoomInfo("M-112 (H117)", 30),
                new TitechRoomInfo("M-119 (H118)", 30),
                new TitechRoomInfo("M-143A (H119A)", 30),
                new TitechRoomInfo("M-143B (H119B)", 36),
                new TitechRoomInfo("M-178 (H1101)", 299),
                new TitechRoomInfo("M-157 (H1102)", 54),
                new TitechRoomInfo("M-156 (H1103)", 30),
                new TitechRoomInfo("M-155 (H1104)", 54),
                new TitechRoomInfo("M-124", 182),
                new TitechRoomInfo("M-134", 30),
                new TitechRoomInfo("M-135", 54),
                // M-2
                new TitechRoomInfo("M-278 (H121)", 286),
                // M-3
                new TitechRoomInfo("M-374 (H131)", 286),
                new TitechRoomInfo("M-356 (H132)", 54),
                // W1-1
                new TitechRoomInfo("W1-102 (W111)", 23),
                new TitechRoomInfo("W1-104 (W112)", 25),
                new TitechRoomInfo("W1-109 (W113)", 18),
                new TitechRoomInfo("W1-111 (W114)", 6),
                // W2-4
                new TitechRoomInfo("W2-401 (W241)", 255),
                new TitechRoomInfo("W2-402 (W242)", 108),
                // W3-2
                new TitechRoomInfo("W3-201 (W321)", 102),
                new TitechRoomInfo("W3-205 (W322)", 41),
                new TitechRoomInfo("W3-207 (W333)", 101),
                // W3-3
                new TitechRoomInfo("W3-301 (W331)", 102),
                new TitechRoomInfo("W3-305 (W332)", 40),
                // W3-5
                new TitechRoomInfo("W3-501 (W351)", 102),
                // W3-7
                new TitechRoomInfo("W3-707 (W371)", 70),
                // W5-1
                new TitechRoomInfo("W5-104", 44),
                new TitechRoomInfo("W5-105", 44),
                new TitechRoomInfo("W5-106", 70),
                new TitechRoomInfo("W5-107", 72),
                // W8E-1
                new TitechRoomInfo("W8E-101", 160),
                // W8E-3
                new TitechRoomInfo("W8E-306 (W832)", 39),
                new TitechRoomInfo("W8E-307 (W833)", 77),
                new TitechRoomInfo("W8E-308 (W834)", 61),
                // W9-2
                new TitechRoomInfo("W9-201 (W921)", 45),
                new TitechRoomInfo("W9-202 (W922)", 30),
                // W9-3
                new TitechRoomInfo("W9-322 (W931)", 54),
                new TitechRoomInfo("W9-323 (W932)", 72),
                new TitechRoomInfo("W9-324 (W933)", 150),
                new TitechRoomInfo("W9-325 (W934)", 81),
                new TitechRoomInfo("W9-326 (W935)", 90),
                new TitechRoomInfo("W9-327 (W936)", 55),
                new TitechRoomInfo("W9-321 (W937)", 20),
                // WL1
                new TitechRoomInfo("WL1-201 (W521)", 269),
                new TitechRoomInfo("WL1-301 (W531)", 273),
                new TitechRoomInfo("WL1-401 (W541)", 269),
                // WL2
                new TitechRoomInfo("WL2-101 (W611)", 108),
                new TitechRoomInfo("WL2-201 (W621)", 143),
                new TitechRoomInfo("WL2-301 (W631)", 143),
                new TitechRoomInfo("WL2-401 (W641)", 143),
                // S2
                new TitechRoomInfo("S2-204 (S221)", 173),
                new TitechRoomInfo("S2-203 (S222)", 186),
                new TitechRoomInfo("S2-202 (S223)", 75),
                new TitechRoomInfo("S2-201 (S224)", 75),
                // S3
                new TitechRoomInfo("S3-215 (S321)", 54),
                new TitechRoomInfo("S3-207 (S322)", 40),
                new TitechRoomInfo("S3-206 (S323)", 40),
                // S4
                new TitechRoomInfo("S4-201 (S421)", 102),
                new TitechRoomInfo("S4-202 (S422)", 72),
                new TitechRoomInfo("S4-203 (S423)", 72),
                // SL
                new TitechRoomInfo("SL-101 (S011)", 165),
                // I1
                new TitechRoomInfo("I1-256 (I121)", 120),
                new TitechRoomInfo("I1-255 (I123)", 60),
                new TitechRoomInfo("I1-254 (I124)", 60),
                // I3
                new TitechRoomInfo("I3-107 (I311)", 78),
                new TitechRoomInfo("I3-203 (I321)", 78),
            ],
            r => r.Name.ToString()
        );

        public static readonly FrozenSet<string> ReservableRoomNames = [
            // M
            "M-278", "M-356", "M-374",
            // W2
            "W2-401", "W2-402",
            // W5
            "W5-104", "W5-105", "W5-106", "W5-107",
            // WL1
            "WL1-201", "WL1-401",
            // WL2
            "WL2-101", "WL2-201", "WL2-301", "WL2-401",
            // S2
            "S2-201", "S2-202", "S2-203", "S2-204",
            // S3
            "S3-206", "S3-207", "S3-215",
            // I1
            "I1-255",
            // I3
            "I3-107", "I3-203"
        ]; 
    }
}
