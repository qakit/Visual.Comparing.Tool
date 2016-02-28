using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace VCT.Test
{
	public class VCTCore
	{
		private readonly RemoteWebDriver _driver;
		private string OutputFolder = @"F:\Projects\VCT.Test\Output";

		public VCTCore(RemoteWebDriver driver)
		{
			_driver = driver;
		}

		/// <summary>
		/// Makes screenshot of specified <see cref="IWebElement"/>
		/// </summary>
		public void MakeElementScreenshot(IWebElement element)
		{
			//Take screenshot as bytearray
			var screenshotBytes = MakeScreen();
			var bitmap = new Bitmap(new MemoryStream(screenshotBytes));
			//Calculate crop area
			var cropRectangle = new Rectangle(element.Location.X, element.Location.Y, element.Size.Width + 1, element.Size.Height);
			Bitmap elementScreen = bitmap.Clone(cropRectangle, bitmap.PixelFormat);
			string outputFile = Path.Combine(OutputFolder, "test.png");
			elementScreen.Save(outputFile, ImageFormat.Png);
		}

		/// <summary>
		/// Makes screenshot of the visible area
		/// </summary>
		/// <returns></returns>
		private byte[] MakeScreen()
		{
			var screenshot = ((ITakesScreenshot)_driver).GetScreenshot().AsByteArray;
			return screenshot;
		}

		public void MakeFullScreenshot()
		{
			var bmp = MakeFullPageScreenshot();
			bmp.Save(Path.Combine(OutputFolder, "fullpage2.png"), ImageFormat.Png);
		}

		private Bitmap MakeFullPageScreenshot()
		{
			byte[] screenBytes = MakeScreen();
			using (var memoryStream = new MemoryStream(screenBytes))
			{
				var image = new Bitmap(memoryStream);
				//TODO exclude scrollbars from the page screenshot
				int initialScreenshotWidth = image.Width;
				int initialScreenshotHeight = image.Height;

				var scrollHeight = unchecked((int)Convert.ToInt64(_driver.ExecuteScript(
						"return Math.max(document.body.scrollHeight, document.documentElement.scrollHeight, " +
						"document.body.offsetHeight, document.documentElement.offsetHeight, " +
						"document.body.clientHeight, document.documentElement.clientHeight);")));
				double devicePixelRatio = Convert.ToDouble(_driver.ExecuteScript("var pr = window.devicePixelRatio; if (pr != undefined && pr != null)return pr; else return 1.0;"));

				var adaptedInitialScreenshotHeight = (int)(initialScreenshotHeight / devicePixelRatio);

				if (Math.Abs(adaptedInitialScreenshotHeight - scrollHeight) <= 40)
				{
					return image;
				}
				int scrollOffset = adaptedInitialScreenshotHeight;
				int times = scrollHeight / adaptedInitialScreenshotHeight;
				int leftover = scrollHeight % adaptedInitialScreenshotHeight;

				var tiledImage = new Bitmap(initialScreenshotWidth, (int)(scrollHeight * devicePixelRatio),
					PixelFormat.Format32bppRgb);
				Graphics graphicsTileImage = Graphics.FromImage(tiledImage);
				graphicsTileImage.DrawImage(image, 0, 0);
				int scroll = 0;
				for (int i = 0; i < times - 1; i++)
				{
					scroll += scrollOffset;
					ScrollHelper.ScrollVerticallyTo(_driver, scroll);
					byte[] nextImageBytes = MakeScreen();
					using (var ms = new MemoryStream(nextImageBytes))
					{
						var nextImage = new Bitmap(ms);
						graphicsTileImage.DrawImage(nextImage, 0, (i + 1) * initialScreenshotHeight);
					}
				}
				if (leftover > 0)
				{
					scroll += scrollOffset;
					ScrollHelper.ScrollVerticallyTo(_driver, scroll);
					byte[] nextImageBytes = MakeScreen();
					using (var ms = new MemoryStream(nextImageBytes))
					{
						var nextImage = new Bitmap(ms);
						Bitmap lastPart =
							nextImage.Clone(
								new Rectangle(0, nextImage.Height - (int)(leftover * devicePixelRatio), nextImage.Width, leftover),
								PixelFormat.Format32bppRgb);
						graphicsTileImage.DrawImage(lastPart, 0, times * initialScreenshotHeight);
					}
				}
				ScrollHelper.ScrollVerticallyTo(_driver, 0);
				return tiledImage;
			}
		}

		public static class ScrollHelper
		{
			public static void ScrollVerticallyTo(RemoteWebDriver driver, int scroll)
			{
				driver.ExecuteScript("window.scrollTo(0, " + scroll + ");");
				try
				{
					WaitUntilItIsScrolledToPosition(driver, scroll);
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
			}

			private static void WaitUntilItIsScrolledToPosition(RemoteWebDriver driver, int scrollPosition)
			{
				int hardTime = 1000;
				if (hardTime > 0)
				{
					Thread.Sleep(hardTime);
				}

				int time = 250;
				bool isScrolledToPosition = false;
				while (time >= 0 && !isScrolledToPosition)
				{
					Thread.Sleep(50);
					time -= 50;
					isScrolledToPosition = Math.Abs(ObtainVerticalScrollPosition(driver) - scrollPosition) < 3;
				}
			}

			private static double ObtainVerticalScrollPosition(RemoteWebDriver driver)
			{
				return Convert.ToDouble(driver.ExecuteScript("return (window.pageYOffset !== undefined) ? window.pageYOffset : (document.documentElement || document.body.parentNode || document.body).scrollTop;"));
			}
		}


	}
}
