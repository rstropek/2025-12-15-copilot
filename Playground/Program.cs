// See https://aka.ms/new-console-template for more information

const int RectWidth = 20;
const int RectHeight = 6;
const ConsoleColor BorderColor = ConsoleColor.Red;

DrawRectangleToConsole(RectWidth, RectHeight, BorderColor);

static void DrawRectangleToConsole(int width, int height, ConsoleColor borderColor)
{
	if (width < 2)
	{
		throw new ArgumentOutOfRangeException(nameof(width), "Width must be >= 2 (needs left/right borders)."
		);
	}

	if (height < 2)
	{
		throw new ArgumentOutOfRangeException(nameof(height), "Height must be >= 2 (needs top/bottom borders)."
		);
	}

	const char TopLeft = '┌';
	const char TopRight = '┐';
	const char BottomLeft = '└';
	const char BottomRight = '┘';
	const char Horizontal = '─';
	const char Vertical = '│';

	string horizontal = new(Horizontal, width - 2);

	var originalColor = Console.ForegroundColor;
	try
	{
		Console.ForegroundColor = borderColor;
		Console.Write(TopLeft);
		Console.Write(horizontal);
		Console.Write(TopRight);
		Console.WriteLine();

		for (int row = 0; row < height - 2; row++)
		{
			Console.ForegroundColor = borderColor;
			Console.Write(Vertical);

			Console.ForegroundColor = originalColor;
			Console.Write(new string(' ', width - 2));

			Console.ForegroundColor = borderColor;
			Console.Write(Vertical);
			Console.WriteLine();
		}

		Console.ForegroundColor = borderColor;
		Console.Write(BottomLeft);
		Console.Write(horizontal);
		Console.Write(BottomRight);
		Console.WriteLine();
	}
	finally
	{
		Console.ForegroundColor = originalColor;
	}
}
