using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UndertaleModLib;

namespace UndertaleModTool.Android;

[Activity(
    MainLauncher = true,
    Exported = true,
    Theme = "@android:style/Theme.Material.NoActionBar"
)]
public class MainActivity : Activity
{
    const int OpenFileRequest = 1001;

    UndertaleData? gameData;
    string? currentFileName;

    LinearLayout? resourcePanel;
    LinearLayout? detailPanel;
    TextView? statusText;
    TextView? fileText;
    EditText? searchBox;

    string currentCategory = "";

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        BuildUI();
    }

    void BuildUI()
    {
        var root = new LinearLayout(this)
        {
            Orientation = Orientation.Vertical
        };

        root.SetBackgroundColor(Color.Rgb(18, 18, 18));

        // ===== TOP BAR =====

        var toolbar = new LinearLayout(this)
        {
            Orientation = Orientation.Horizontal
        };

        toolbar.SetPadding(12, 8, 12, 4);

        var title = MakeText("UndertaleModTool", 21);
        title.SetTextColor(Color.White);

        toolbar.AddView(
            title,
            new LinearLayout.LayoutParams(0, -2, 1)
        );

        var openButton = MakeButton("OPEN");
        openButton.Click += (_, _) => OpenFile();

        toolbar.AddView(openButton);

        root.AddView(toolbar);

        // ===== FILE NAME =====

        fileText = MakeText(
            "No GameMaker data file opened",
            13
        );

        fileText.SetTextColor(Color.LightGray);
        fileText.SetPadding(14, 0, 14, 8);

        root.AddView(fileText);

        // ===== SEARCH =====

        searchBox = new EditText(this)
        {
            Hint = "Search resources..."
        };

        searchBox.SetTextColor(Color.White);
        searchBox.SetHintTextColor(Color.Gray);

        searchBox.TextChanged += (_, _) =>
        {
            if (!string.IsNullOrEmpty(currentCategory))
                ShowResources(currentCategory);
        };

        root.AddView(
            searchBox,
            new LinearLayout.LayoutParams(-1, -2)
        );

        // ===== MAIN AREA =====

        var main = new LinearLayout(this)
        {
            Orientation = Orientation.Horizontal
        };

        main.SetPadding(8, 4, 8, 4);

        // Resource list

        resourcePanel = new LinearLayout(this)
        {
            Orientation = Orientation.Vertical
        };

        resourcePanel.SetBackgroundColor(
            Color.Rgb(28, 28, 28)
        );

        var resourceScroll = new ScrollView(this);
        resourceScroll.AddView(resourcePanel);

        main.AddView(
            resourceScroll,
            new LinearLayout.LayoutParams(
                0,
                0,
                0.40f
            )
        );

        // Details

        detailPanel = new LinearLayout(this)
        {
            Orientation = Orientation.Vertical
        };

        detailPanel.SetPadding(14, 10, 10, 10);

        var detailScroll = new ScrollView(this);
        detailScroll.AddView(detailPanel);

        main.AddView(
            detailScroll,
            new LinearLayout.LayoutParams(
                0,
                0,
                0.60f
            )
        );

        root.AddView(
            main,
            new LinearLayout.LayoutParams(
                -1,
                0,
                1
            )
        );

        // ===== STATUS =====

        statusText = MakeText(
            "Ready",
            12
        );

        statusText.SetTextColor(Color.LightGray);
        statusText.SetPadding(14, 5, 14, 10);

        root.AddView(statusText);

        SetContentView(root);

        ShowCategories();
        ShowDetails(
            "Welcome",
            "Open a GameMaker data file to begin."
        );
    }

    TextView MakeText(
        string text,
        float size
    )
    {
        var view = new TextView(this)
        {
            Text = text,
            TextSize = size
        };

        view.SetPadding(8, 8, 8, 8);

        return view;
    }

    Button MakeButton(string text)
    {
        var button = new Button(this)
        {
            Text = text
        };

        return button;
    }

    void OpenFile()
    {
        var intent = new Intent(
            Intent.ActionOpenDocument
        );

        intent.AddCategory(
            Intent.CategoryOpenable
        );

        intent.SetType("*/*");

        StartActivityForResult(
            intent,
            OpenFileRequest
        );
    }

    protected override void OnActivityResult(
        int requestCode,
        Result resultCode,
        Intent? data
    )
    {
        base.OnActivityResult(
            requestCode,
            resultCode,
            data
        );

        if (
            requestCode != OpenFileRequest ||
            resultCode != Result.Ok ||
            data?.Data == null
        )
        {
            return;
        }

        var uri = data.Data;

        currentFileName =
            uri.LastPathSegment ??
            "GameMaker data";

        fileText!.Text =
            "Opening: " + currentFileName;

        SetStatus(
            "Loading GameMaker data..."
        );

        _ = Task.Run(
            () => LoadGame(uri)
        );
    }

    void LoadGame(
        Android.Net.Uri uri
    )
    {
        try
        {
            using var stream =
                ContentResolver!.OpenInputStream(uri);

            if (stream == null)
                throw new IOException(
                    "Could not open file."
                );

            var loaded =
                UndertaleIO.Read(
                    stream
                );

            RunOnUiThread(() =>
            {
                gameData?.Dispose();

                gameData = loaded;

                fileText!.Text =
                    "Loaded: " + currentFileName;

                ShowCategories();

                ShowDetails(
                    "Game loaded",
                    "Choose a resource category."
                );

                SetStatus(
                    "Loaded successfully."
                );
            });
        }
        catch (Exception ex)
        {
            RunOnUiThread(() =>
            {
                SetStatus(
                    "Error: " + ex.Message
                );

                Toast.MakeText(
                    this,
                    "Could not open file.",
                    ToastLength.Long
                )?.Show();
            });
        }
    }

    void ShowCategories()
    {
        if (resourcePanel == null)
            return;

        resourcePanel.RemoveAllViews();

        AddCategory(
            "Sprites",
            gameData?.Sprites?.Count ?? 0
        );

        AddCategory(
            "Rooms",
            gameData?.Rooms?.Count ?? 0
        );

        AddCategory(
            "Objects",
            gameData?.GameObjects?.Count ?? 0
        );

        AddCategory(
            "Scripts",
            gameData?.Scripts?.Count ?? 0
        );

        AddCategory(
            "Sounds",
            gameData?.Sounds?.Count ?? 0
        );

        AddCategory(
            "Fonts",
            gameData?.Fonts?.Count ?? 0
        );
    }

    void AddCategory(
        string name,
        int count
    )
    {
        var button = MakeButton(
            $"{name}\n{count} resources"
        );

        button.Gravity =
            GravityFlags.Left;

        button.Click += (_, _) =>
        {
            currentCategory = name;

            if (searchBox != null)
                searchBox.Text = "";

            ShowResources(name);
        };

        resourcePanel?.AddView(button);
    }

    void ShowResources(
        string category
    )
    {
        if (
            resourcePanel == null ||
            gameData == null
        )
        {
            return;
        }

        resourcePanel.RemoveAllViews();

        var back = MakeButton("← Categories");

        back.Click += (_, _) =>
        {
            currentCategory = "";

            ShowCategories();

            ShowDetails(
                "Resource Browser",
                "Choose a category."
            );
        };

        resourcePanel.AddView(back);

        IList? list = category switch
        {
            "Sprites" =>
                gameData.Sprites,

            "Rooms" =>
                gameData.Rooms,

            "Objects" =>
                gameData.GameObjects,

            "Scripts" =>
                gameData.Scripts,

            "Sounds" =>
                gameData.Sounds,

            "Fonts" =>
                gameData.Fonts,

            _ => null
        };

        if (list == null)
            return;

        string search =
            searchBox?.Text?
                .Trim()
                .ToLowerInvariant()
            ?? "";

        foreach (
            var item in list.Cast<object>()
        )
        {
            string name =
                item?.ToString()
                ?? "(unnamed)";

            if (
                search.Length > 0 &&
                !name.ToLowerInvariant()
                    .Contains(search)
            )
            {
                continue;
            }

            var button =
                MakeButton(name);

            button.Gravity =
                GravityFlags.Left;

            button.Click += (_, _) =>
            {
                ShowDetails(
                    name,
                    $"Type: {category}\n\n" +
                    "Resource selected.\n\n" +
                    "The editor for this resource " +
                    "will be added in the next stages."
                );
            };

            resourcePanel.AddView(button);
        }

        SetStatus(
            $"{category}: {list.Count} resources"
        );
    }

    void ShowDetails(
        string title,
        string description
    )
    {
        if (detailPanel == null)
            return;

        detailPanel.RemoveAllViews();

        var titleView =
            MakeText(title, 22);

        titleView.SetTextColor(
            Color.White
        );

        detailPanel.AddView(
            titleView
        );

        var separator =
            MakeText(
                "────────────────",
                12
            );

        separator.SetTextColor(
            Color.Gray
        );

        detailPanel.AddView(
            separator
        );

        var body =
            MakeText(
                description,
                15
            );

        body.SetTextColor(
            Color.LightGray
        );

        detailPanel.AddView(
            body
        );
    }

    void SetStatus(
        string message
    )
    {
        if (statusText != null)
            statusText.Text = message;
    }

    protected override void OnDestroy()
    {
        gameData?.Dispose();
        gameData = null;

        base.OnDestroy();
    }
}
