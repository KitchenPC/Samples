using System.Text.Json;

namespace KitchenPC.Samples.ShoppingList;

public sealed class ShoppingListStore
{
   private static readonly JsonSerializerOptions SerializerOptions = new()
   {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      WriteIndented = true,
   };

   public ShoppingListStore(string? path = null)
   {
      FilePath = path ?? GetDefaultPath();
   }

   public string FilePath { get; }

   public ShoppingListDocument Load()
   {
      if (!File.Exists(FilePath))
         return new ShoppingListDocument();

      try
      {
         var json = File.ReadAllText(FilePath);
         var document = JsonSerializer.Deserialize<ShoppingListDocument>(json, SerializerOptions);

         if (document is null)
            throw new InvalidDataException("The shopping-list file contains no data.");

         if (document.Version != ShoppingListDocument.CurrentVersion)
            throw new InvalidDataException(
               $"Unsupported shopping-list version {document.Version}."
            );

         document.Entries ??= [];
         return document;
      }
      catch (JsonException exception)
      {
         throw new InvalidDataException(
            $"The shopping-list file is not valid JSON: {FilePath}",
            exception
         );
      }
   }

   public void Save(ShoppingListDocument document)
   {
      ArgumentNullException.ThrowIfNull(document);

      var directory = Path.GetDirectoryName(FilePath);
      if (string.IsNullOrWhiteSpace(directory))
         throw new InvalidOperationException("The shopping-list path has no parent directory.");

      Directory.CreateDirectory(directory);
      var temporaryPath = FilePath + ".tmp";

      try
      {
         var json = JsonSerializer.Serialize(document, SerializerOptions);
         File.WriteAllText(temporaryPath, json);
         File.Move(temporaryPath, FilePath, overwrite: true);
      }
      finally
      {
         if (File.Exists(temporaryPath))
            File.Delete(temporaryPath);
      }
   }

   public static string GetDefaultPath()
   {
      var applicationData = Environment.GetFolderPath(
         Environment.SpecialFolder.LocalApplicationData
      );

      if (string.IsNullOrWhiteSpace(applicationData) && OperatingSystem.IsLinux())
      {
         applicationData = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
         if (string.IsNullOrWhiteSpace(applicationData))
         {
            applicationData = Path.Combine(GetUserProfile(), ".local", "share");
         }
      }

      if (string.IsNullOrWhiteSpace(applicationData) && OperatingSystem.IsMacOS())
      {
         applicationData = Path.Combine(GetUserProfile(), "Library", "Application Support");
      }

      if (string.IsNullOrWhiteSpace(applicationData))
      {
         applicationData = Path.Combine(GetUserProfile(), "AppData", "Local");
      }

      return Path.Combine(
         applicationData,
         "KitchenPC",
         "Samples",
         "ShoppingList",
         "shopping-list.json"
      );
   }

   private static string GetUserProfile()
   {
      var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
      if (string.IsNullOrWhiteSpace(userProfile))
         throw new InvalidOperationException("Could not determine the current user's profile.");

      return userProfile;
   }
}
