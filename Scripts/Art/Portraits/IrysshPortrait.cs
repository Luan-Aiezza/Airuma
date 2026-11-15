using System;
using P = PixelColor;

/// <summary>
/// The face of the Iryssh, the boss of stage 1, for the dialogue box: the neutral face drawn by the author (<see cref="IrysshPortraitData"/>, 128 x 128) and the other expressions made from it by redrawing the eyes, the brows and the
/// mouth in the colours of the drawing itself (the hair, the crown and the rest are the author's pixels). A window of 96 x 96 of the drawing is shown at 4/3 (128 x 128), the same as Atli's; the background is clear and the container
/// puts it over its gradient.
/// </summary>
public static class IrysshPortrait
{
    public const int Size = 128;

    // The window of the drawing that is shown: its top left corner and its side (the crown to the chin, hair to hair).
    private const int WindowX = 16, WindowY = 32, WindowSize = 96;

    // Where the features are in the drawing: the eyes (x ranges; rows 82 to 90), the brows (rows 79 to 81) and the mouth.
    private const int LeftX0 = 42, LeftX1 = 58, RightX0 = 70, RightX1 = 86;

    public static PixelCanvas Make(Emotion emotion = Emotion.Neutral)
    {
        int W = IrysshPortraitData.Width, H = IrysshPortraitData.Height;
        var g = new char[H][];
        for (int y = 0; y < H; y++) g[y] = IrysshPortraitData.Rows[y].ToCharArray();
        void Set(int x, int y, char ch) { if (x >= 0 && y >= 0 && x < W && y < H) g[y][x] = ch; }
        void Rect(int x0, int y0, int x1, int y1, char ch) { for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) Set(x, y, ch); }
        void Line(int x0, int y0, int x1, int y1, char ch, int thick = 1)
        {
            int n = Math.Max(Math.Abs(x1 - x0), Math.Abs(y1 - y0));
            for (int i = 0; i <= n; i++)
            {
                int x = n == 0 ? x0 : x0 + (x1 - x0) * i / n, y = n == 0 ? y0 : y0 + (y1 - y0) * i / n;
                for (int t = 0; t < thick; t++) Set(x, y + t, ch);
            }
        }
        // the brows are thin strokes of the hair's colours at rows 80 and 81; they are rubbed out (the shade of the crown above stays) and redrawn
        void ClearBrows()
        {
            foreach (var (x0, x1) in new[] { (LeftX0, LeftX1), (RightX0, RightX1) })
                for (int y = 79; y <= 82; y++) for (int x = x0; x <= x1; x++) if (g[y][x] == 'B' || g[y][x] == 'd' || g[y][x] == 'c') g[y][x] = y <= 80 ? 'p' : 'P';
        }
        void Brow(int ox, int oy, int ix, int iy) { Line(ox, oy, ix, iy, 'd', 2); }
        void ClearMouth() => Rect(55, 105, 72, 110, 'P');
        // heavy lids: the dark band over the top of each eye, `rows` rows more
        void Lid(int x0, int x1, int rows) { for (int y = 85; y < 85 + rows; y++) for (int x = x0 + 3; x <= x1 - 3; x++) Set(x, y, 'K'); }
        void ClosedEye(int x0, int x1)
        {
            Rect(x0 - 1, 82, x1 + 1, 91, 'P');
            for (int x = x0 + 1; x <= x1 - 1; x++)
            {
                double u = (x - x0 - 1) / (double)(x1 - x0 - 2);
                int y = 89 - (int)Math.Round(4 * Math.Sin(Math.PI * u));
                Set(x, y, 'K'); Set(x, y + 1, 'K');
            }
            Set(x0, 88, 'K'); Set(x1, 88, 'K');
        }
        // wide eyes: the upper lid goes up two rows and the pale of the eye shows where it was
        void WideEye(int x0, int x1)
        {
            for (int y = 83; y <= 84; y++) for (int x = x0 + 2; x <= x1 - 2; x++) { Set(x, y - 2, g[y][x]); Set(x, y, 'y'); }
        }

        switch (emotion)
        {
            case Emotion.Happy:
                ClosedEye(LeftX0, LeftX1); ClosedEye(RightX0, RightX1);
                ClearBrows(); Brow(44, 80, 56, 79); Brow(84, 80, 72, 79);
                ClearMouth();
                for (int x = 57; x <= 70; x++)             // an open smile with teeth
                {
                    double u = (x - 57) / 13.0;
                    int bottom = 106 + (int)Math.Round(4 * Math.Sin(Math.PI * u));
                    Set(x, 106, 'b');
                    for (int y = 107; y <= bottom; y++) Set(x, y, y == 107 ? 'T' : 'E');
                    Set(x, bottom + 1, 'b');
                }
                break;
            case Emotion.Surprised:
                WideEye(LeftX0, LeftX1); WideEye(RightX0, RightX1);
                ClearBrows(); Brow(44, 77, 56, 76); Brow(84, 77, 72, 76);
                ClearMouth();
                for (int y = 104; y <= 111; y++) for (int x = 60; x <= 67; x++)
                {
                    double dx = (x - 63.5) / 4.0, dy = (y - 107.5) / 4.2, d = dx * dx + dy * dy;
                    if (d <= 1) Set(x, y, d > 0.6 ? 'b' : 'E');
                }
                break;
            case Emotion.Annoyed:
                Lid(LeftX0, LeftX1, 2); Lid(RightX0, RightX1, 2);
                ClearBrows(); Brow(43, 79, 57, 82); Brow(85, 79, 71, 82);        // the inner ends go down
                ClearMouth();
                Line(59, 108, 68, 108, 'b', 2); Set(58, 109, 'b'); Set(69, 109, 'b'); Set(57, 110, 'b'); Set(70, 110, 'b');
                break;
            case Emotion.Worried:
                ClearBrows(); Brow(43, 82, 57, 79); Brow(85, 82, 71, 79);         // the inner ends go up
                ClearMouth();
                for (int x = 58; x <= 69; x++)
                {
                    double u = (x - 58) / 11.0;
                    int y = 110 - (int)Math.Round(2 * Math.Sin(Math.PI * u));
                    Set(x, y, 'b'); Set(x, y + 1, 'B');
                }
                break;
            case Emotion.Sheepish:
                Lid(LeftX0, LeftX1, 1); Lid(RightX0, RightX1, 1);                // eyes lowered
                ClearBrows(); Brow(44, 81, 57, 79); Brow(84, 81, 71, 79);
                for (int x = 47; x <= 55; x++) for (int y = 95; y <= 97; y++) Set(x, y, 'c');       // a blush on the cheeks
                for (int x = 73; x <= 81; x++) for (int y = 95; y <= 97; y++) Set(x, y, 'c');
                ClearMouth();
                for (int x = 58; x <= 69; x++) Set(x, 108 + ((x / 2) % 2), 'b');                    // a wavy little mouth
                break;
        }

        // The window is zoomed 4/3 (nearest pixel); what is clear in the drawing stays clear.
        var c = new PixelCanvas(Size, Size);
        for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                char ch = g[WindowY + y * WindowSize / Size][WindowX + x * WindowSize / Size];
                if (ch != '.') c[x, y] = IrysshPortraitData.Palette[ch];
            }
        return c;
    }
}
