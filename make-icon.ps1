$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$stream = [IO.File]::Create((Join-Path $PSScriptRoot 'Aurora.ico'))
$writer = [IO.BinaryWriter]::new($stream)
try {
    $sizes = @(16, 32, 48, 256)
    $frames = foreach ($size in $sizes) {
        $bitmap = [Drawing.Bitmap]::new($size, $size)
        $canvas = [Drawing.Graphics]::FromImage($bitmap)
        $canvas.SmoothingMode = 'AntiAlias'
        $canvas.ScaleTransform($size / 256.0, $size / 256.0)
        $bounds = [Drawing.Rectangle]::new(8, 8, 240, 240)
        $tile = [Drawing.Drawing2D.GraphicsPath]::new()
        $tile.AddArc(8, 8, 104, 104, 180, 90)
        $tile.AddArc(144, 8, 104, 104, 270, 90)
        $tile.AddArc(144, 144, 104, 104, 0, 90)
        $tile.AddArc(8, 144, 104, 104, 90, 90)
        $tile.CloseFigure()
        $brush = [Drawing.Drawing2D.LinearGradientBrush]::new($bounds, [Drawing.Color]::FromArgb(15, 24, 43), [Drawing.Color]::FromArgb(34, 23, 68), 45.0)
        $canvas.FillPath($brush, $tile)
        $pen = [Drawing.Pen]::new([Drawing.Color]::FromArgb(73, 79, 128), 3)
        $canvas.DrawPath($pen, $tile)
        $mark = [Drawing.Drawing2D.LinearGradientBrush]::new($bounds, [Drawing.Color]::FromArgb(104, 250, 236), [Drawing.Color]::FromArgb(147, 104, 255), 65.0)
        function Paint-Polygon([int[]]$coordinates) {
            $points = for ($i = 0; $i -lt $coordinates.Length; $i += 2) {
                [Drawing.Point]::new($coordinates[$i], $coordinates[$i + 1])
            }
            $canvas.FillPolygon($mark, [Drawing.Point[]]$points)
        }
        Paint-Polygon @(46, 183, 114, 62, 137, 62, 211, 183, 176, 183, 125, 98, 80, 183)
        Paint-Polygon @(100, 161, 148, 143, 164, 168, 86, 187)
        if ($size -eq 256) { $bitmap.Save((Join-Path $PSScriptRoot 'Aurora-icon.png'), [Drawing.Imaging.ImageFormat]::Png) }
        $mark.Dispose()
        $tile.Dispose()
        $memory = [IO.MemoryStream]::new()
        $bitmap.Save($memory, [Drawing.Imaging.ImageFormat]::Png)
        @{Size=$size; Bytes=$memory.ToArray()}
        $memory.Dispose()
        $pen.Dispose()
        $brush.Dispose()
        $canvas.Dispose()
        $bitmap.Dispose()
    }
    $writer.Write([uint16]0)
    $writer.Write([uint16]1)
    $writer.Write([uint16]$frames.Count)
    $offset = 6 + 16 * $frames.Count
    foreach ($frame in $frames) {
        $writer.Write([byte]($frame.Size % 256))
        $writer.Write([byte]($frame.Size % 256))
        $writer.Write([uint16]0)
        $writer.Write([uint16]1)
        $writer.Write([uint16]32)
        $writer.Write([uint32]$frame.Bytes.Length)
        $writer.Write([uint32]$offset)
        $offset += $frame.Bytes.Length
    }
    foreach ($frame in $frames) { $writer.Write([byte[]]$frame.Bytes) }
} finally {
    $writer.Dispose()
}
