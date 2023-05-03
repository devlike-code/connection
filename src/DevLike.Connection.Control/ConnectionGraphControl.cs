using Eto.Forms;

namespace DevLike.Connection.Control;

public class ConnectionGraphControl : Drawable
{
    private readonly FileFilter _grasmFileFilter = new FileFilter("GRASM layouts (*.grasm)", new[] { ".grasm" });

    private GraphEditorTrait editor;
    private WinformsGraphics graphics;

    public void Clear()
    {
        editor.Reset();
    }

    public void StartSave()
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Title = "Save Graph";
        saveFileDialog.Filters.Add(_grasmFileFilter);
        saveFileDialog.CurrentFilter = _grasmFileFilter;

        var result = saveFileDialog.ShowDialog(this);

        if (result == DialogResult.Ok)
        {
            var filePath = saveFileDialog.FileName;
            if (saveFileDialog.CurrentFilter == _grasmFileFilter && !filePath.EndsWith(".grasm"))
                filePath += ".grasm";

            editor.SaveToFile(filePath);
        }
    }

    public void StartLoad()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Title = "Open Graph";
        openFileDialog.Filters.Add(_grasmFileFilter);
        openFileDialog.CurrentFilter = _grasmFileFilter;
        var result = openFileDialog.ShowDialog(this);

        if (result == DialogResult.Ok)
        {
            try
            {
                editor.LoadFromFile(openFileDialog.FileName);
            }
            catch { }
        }

    }

    public ConnectionGraphControl()
    {
        editor = new GraphEditorTrait();
        graphics = new WinformsGraphics();

        Paint += ConnectionGraphControl_Paint;
        TextInput += ConnectionGraphControl_KeyPress;
        KeyUp += ConnectionGraphControl_KeyUp;
        KeyDown += ConnectionGraphControl_KeyDown;
        MouseDoubleClick += ConnectionGraphControl_MouseDoubleClick;
        MouseDown += ConnectionGraphControl_MouseDown;
        MouseMove += ConnectionGraphControl_MouseMove;
        MouseUp += ConnectionGraphControl_MouseUp;

        Application.Instance.AsyncInvoke(DrawLoop);
    }

    async void DrawLoop()
    {
        while (true)
        {
            Invalidate();
            await Task.Delay(10);
        }
    }

    private void ConnectionGraphControl_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Keys.Escape || e.Key == Keys.Enter)
        {
            editor.Logic.Trigger("esc");
        }
        else if (e.Key == Keys.S && e.Modifiers.HasFlag(Keys.Control))
        {
            StartSave();
        }
        else if (e.Key == Keys.O && e.Modifiers.HasFlag(Keys.Control))
        {
            StartLoad();
        }
        else if (e.Key == Keys.Delete)
        {
            editor.DeleteSelected();
        }

        editor.IsLinkingBothWays = e.Modifiers.HasFlag(Keys.Shift);
    }

    private void ConnectionGraphControl_MouseClick(object? sender, MouseEventArgs e)
    {
        if (GraphInternals.Hovered.Count == 0)
        {
            editor.Logic.Trigger("click empty");
        }
        else
        {
            editor.Logic.Trigger("click node");
        }
    }

    private void ConnectionGraphControl_MouseDoubleClick(object? sender, MouseEventArgs e)
    {
        if (e.Buttons == MouseButtons.Primary)
        {
            editor.Logic.Reset();
            if (GraphInternals.Hovered.Count == 0)
            {
                editor.Logic.Trigger("dblclick empty");
            }
            else
            {
                editor.Logic.Trigger("dblclick node");
            }
        }
    }

    private void ConnectionGraphControl_MouseDown(object? sender, MouseEventArgs e)
    {
        if (GraphInternals.Hovered.Count == 0)
        {
            if (e.Buttons == MouseButtons.Primary)
            {
                editor.Logic.Trigger("mousedown left empty");
            }
            else
            {
                editor.Logic.Trigger("mousedown right empty");
            }
        }
        else
        {
            if (e.Buttons == MouseButtons.Primary)
            {
                editor.Logic.Trigger("mousedown left node");
            }
            else
            {
                editor.Logic.Trigger("mousedown right node");
            }
        }
    }

    private void ConnectionGraphControl_MouseMove(object? sender, MouseEventArgs e)
    {
        editor.UpdateMousePosition(new Float2 { X = e.Location.X, Y = e.Location.Y });
        if (editor.Logic.IsRectSelecting())
        {
            if (editor.SelectingRect != null)
            {
                var r = editor.SelectingRect.Value;
                r.W = Rendering.Mouse.X - r.X;
                r.H = Rendering.Mouse.Y - r.Y;
                editor.SelectingRect = r;
            }
        }

        if (editor.Logic.IsMoving)
        {
            var delta = editor.UpdateMouseMoveDelta(Rendering.Mouse);
        }

        editor.IsLinkingBothWays = e.Modifiers.HasFlag(Keys.Shift);
    }

    private void ConnectionGraphControl_MouseUp(object? sender, MouseEventArgs e)
    {
        if (GraphInternals.Hovered.Count == 0)
        {
            if (e.Buttons == MouseButtons.Primary)
            {
                editor.Logic.Trigger("mouseup left empty");
            }
            else
            {
                editor.Logic.Trigger("mouseup right empty");
            }
        }
        else
        {
            if (e.Buttons == MouseButtons.Primary)
            {
                editor.Logic.Trigger("mouseup left node");
            }
            else
            {
                editor.Logic.Trigger("mouseup right node");
            }
        }

        editor.IsLinkingBothWays = e.Modifiers.HasFlag(Keys.Shift);
    }

    private void ConnectionGraphControl_Paint(object? sender, PaintEventArgs e)
    {
        e.Graphics.AntiAlias = true;
        graphics.Graphics = e.Graphics;

        Rendering.DrawGrid(graphics, this.Width, this.Height);

        editor.Nodes.Sort();

        foreach (var node in editor.Nodes)
        {
            if (node.Alive)
                node.Draw(graphics);
        }

        if (editor.ConnectingStartNode != null)
        {
            graphics.DrawArrow(Tint.White, new List<Float2> {
                editor.ConnectingStartNode.Origin, Rendering.Mouse
            }, editor.IsLinkingBothWays);
        }

        if (editor.SelectingRect != null && Math.Abs(editor.SelectingRect.Value.W) >= 2 && Math.Abs(editor.SelectingRect.Value.H) >= 2)
        {
            graphics.DrawRectangle(Tint.White, editor.SelectingRect.Value);
        }

        graphics.DrawText(Tint.Yellow, new Float2 { X = 10, Y = 10 }, $"Moving: {editor.Logic.IsMoving}");
        graphics.DrawText(Tint.Yellow, new Float2 { X = 10, Y = 30 }, $"Editing: {editor.EditingLabelNode}");
    }

    private void ConnectionGraphControl_KeyDown(object? sender, KeyEventArgs e)
    {
        if (editor.EditingLabelNode != null)
        {
            if (e.Key == Keys.Backspace)
            {
                var text = editor.EditingLabelNode.GetTag("Label");
                if (text.Length > 0)
                {
                    text = text.Remove(text.Length - 1);
                }
                editor.EditingLabelNode.AddTag("Label", text);
            }
        }
    }

    private void ConnectionGraphControl_KeyPress(object? sender, TextInputEventArgs e)
    {
        if (editor.EditingLabelNode != null)
        {
            var text = editor.EditingLabelNode.GetTag("Label");
            text += e.Text;
            editor.EditingLabelNode.AddTag("Label", text);
        }
    }
}
