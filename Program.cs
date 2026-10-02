using Android.App;
using Android.Content.PM;
using Android.OS;

namespace UndertaleModTool.Android;

[Activity(
    MainLauncher = true,
    Exported = true,
    Theme = "@android:style/Theme.Material.NoActionBar",
    ScreenOrientation = ScreenOrientation.Unspecified)]
public class MainActivity : Activity
{
    const int OpenFileRequest = 1001;

    Android.Widget.TextView? statusText;
    Android.Widget.TextView? fileText;
    Android.Widget.LinearLayout? resourceList;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        BuildUi();
    }

    void BuildUi()
    {
        var root = new Android.Widget.LinearLayout(this)
        {
            Orientation = Android.Widget.Orientation.Vertical
        };
        root.SetBackgroundColor(Android.Graphics.Color.Rgb(18, 18, 18));

        var toolbar = new Android.Widget.LinearLayout(this)
        {
            Orientation = Android.Widget.Orientation.Horizontal
        };
        toolbar.SetPadding(20, 20, 20, 12);

        var title = MakeText("UndertaleModTool", 22);
        title.SetTextColor(Android.Graphics.Color.White);
        toolbar.AddView(title, new Android.Widget.LinearLayout.LayoutParams(
            0, Android.Views.ViewGroup.LayoutParams.WrapContent, 1));

        var open = MakeButton("OPEN");
        open.Click += (_, _) => OpenGameFile();
        toolbar.AddView(open);

        root.AddView(toolbar);

        fileText = MakeText("No game file opened", 14);
        fileText.SetTextColor(Android.Graphics.Color.LightGray);
        fileText.SetPadding(20, 0, 20, 18);
        root.AddView(fileText);

        var actions = new Android.Widget.LinearLayout(this)
        {
            Orientation = Android.Widget.Orientation.Horizontal
        };
        actions.SetPadding(12, 0, 12, 12);

        foreach (var name in new[] { "SAVE", "SAVE AS", "UNDO", "REDO" })
        {
            var button = MakeButton(name);
            button.Click += (_, _) =>
            {
                statusText!.Text = name + " is not connected to the game parser yet.";
            };
            actions.AddView(button, new Android.Widget.LinearLayout.LayoutParams(
                0, Android.Views.ViewGroup.LayoutParams.WrapContent, 1));
        }

        root.AddView(actions);

        var split = new Android.Widget.LinearLayout(this)
        {
            Orientation = Android.Widget.Orientation.Horizontal
        };
        split.SetPadding(12, 0, 12, 12);

        var categories = new[] { "Sprites", "Rooms", "Objects", "Scripts", "Sounds", "Fonts" };

        resourceList = new Android.Widget.LinearLayout(this)
        {
            Orientation = Android.Widget.Orientation.Vertical
        };
        resourceList.SetBackgroundColor(Android.Graphics.Color.Rgb(30, 30, 30));
        resourceList.SetPadding(8, 8, 8, 8);

        foreach (var category in categories)
        {
            var item = MakeButton(category);
            item.TextSize = 15;
            item.Click += (_, _) =>
            {
                statusText!.Text = category + " selected";
            };
            resourceList.AddView(item);
        }

        var scroll = new Android.Widget.ScrollView(this);
        scroll.AddView(resourceList);
        split.AddView(scroll, new Android.Widget.LinearLayout.LayoutParams(
            0, 0, 0.32f));

        var editor = new Android.Widget.LinearLayout(this)
        {
            Orientation = Android.Widget.Orientation.Vertical
        };
        editor.SetPadding(16, 8, 8, 8);

        var editorTitle = MakeText("Editor", 20);
        editorTitle.SetTextColor(Android.Graphics.Color.White);
        editor.AddView(editorTitle);

        var editorText = new Android.Widget.EditText(this)
        {
            Hint = "Select a resource to edit it...",
            Gravity = Android.Views.GravityFlags.Top | Android.Views.GravityFlags.Start,
            InputType = Android.Text.InputTypes.ClassText |
                        Android.Text.InputTypes.TextFlagMultiLine
        };
        editorText.SetTextColor(Android.Graphics.Color.White);
        editorText.SetHintTextColor(Android.Graphics.Color.Gray);
        editorText.SetBackgroundColor(Android.Graphics.Color.Rgb(25, 25, 25));
        editorText.SetPadding(16, 16, 16, 16);
        editor.AddView(editorText, new Android.Widget.LinearLayout.LayoutParams(
            -1, 0, 1));

        split.AddView(editor, new Android.Widget.LinearLayout.LayoutParams(
            0, 0, 0.68f));

        root.AddView(split, new Android.Widget.LinearLayout.LayoutParams(
            -1, 0, 1));

        statusText = MakeText("Ready — Android UI prototype", 13);
        statusText.SetTextColor(Android.Graphics.Color.LightGray);
        statusText.SetPadding(20, 8, 20, 20);
        root.AddView(statusText);

        SetContentView(root);
    }

    Android.Widget.TextView MakeText(string text, float size)
    {
        var view = new Android.Widget.TextView(this)
        {
            Text = text,
            TextSize = size
        };
        return view;
    }

    Android.Widget.Button MakeButton(string text)
    {
        var button = new Android.Widget.Button(this)
        {
            Text = text
        };
        return button;
    }

    void OpenGameFile()
    {
        var intent = new Android.Content.Intent(Android.Content.Intent.ActionOpenDocument);
        intent.AddCategory(Android.Content.Intent.CategoryOpenable);
        intent.SetType("*/*");
        StartActivityForResult(intent, OpenFileRequest);
    }

    protected override void OnActivityResult(
        int requestCode,
        Android.App.Result resultCode,
        Android.Content.Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);

        if (requestCode != OpenFileRequest ||
            resultCode != Android.App.Result.Ok ||
            data?.Data is null)
            return;

        var uri = data.Data;
        var name = uri.LastPathSegment ?? uri.ToString();

        if (fileText != null)
            fileText.Text = "Opened: " + name;

        if (statusText != null)
            statusText.Text = "File selected. Parser integration comes next.";
    }
}
