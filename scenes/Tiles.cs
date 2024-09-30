using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Tiles : Node2D
{
    TileMapLayer map;
    HSlider slider;
    Timer timer;
    int xStep = 10;
    int yStep = 10;
    Vector2I TileId = new Vector2I(0, 0);
    Vector2I size = new Vector2I(73, 41);
    List<Vector2I> live = new List<Vector2I> { };
    List<Vector2I> audioMap = new List<Vector2I> { };
    List<string> audioPaths = new List<string> {
         "res://audio/0.wav",
         "res://audio/2.wav",
         "res://audio/5.wav",
         "res://audio/7.wav",
         "res://audio/9.wav",
         "res://audio/12.wav",
         "res://audio/14.wav",
         "res://audio/17.wav",
         "res://audio/19.wav",
         "res://audio/21.wav",
         "res://audio/24.wav",
 };

    public override void _Ready()
    {
        map = GetNode<TileMapLayer>("TileMapLayer");
        slider = GetNode<HSlider>("HSlider");
        timer = GetNode<Timer>("Timer");
        audioMap = GenerateNoteNodes(xStep, yStep);
    }
    public override void _Process(double delta)
    {
        if (Input.IsActionPressed("mbleft"))
        {
            Vector2I coords = map.LocalToMap(GetGlobalMousePosition());
            if (coords.Y < 36)
            {
                map.SetCell(map.LocalToMap(GetGlobalMousePosition()), 0, TileId);
            }
        }
    }
    public void Step()
    {
        for (int i = 0; i < GetChildCount(); i++)
        {
            if (GetChild(i) is AudioStreamPlayer audioStreamPlayer)
            {
                audioStreamPlayer.Stop();
                audioStreamPlayer.QueueFree();
            }
        }
        live.Clear();
        for (int i = 0; i < size.X; i++)
        {
            for (int j = 0; j < size.Y; j++)
            {
                int cell = map.GetCellSourceId(new Vector2I(i, j));
                int n = GetNeighbors(i, j);
                if (cell > -1)
                {
                    if (n == 2 || n == 3)
                    {
                        live.Add(new Vector2I(i, j));
                        int indexOfAudioFile = audioMap.IndexOf(new Vector2I(i, j));
                        if (indexOfAudioFile >= 0)
                        {
                            AudioStreamPlayer audio = new AudioStreamPlayer();
                            audio.Stream = ResourceLoader.Load<AudioStream>(audioPaths[indexOfAudioFile % 11]);
                            audio.Finished += audio.QueueFree;
                            AddChild(audio);
                            audio.Play();
                        }
                    }
                }
                else if (n == 3)
                {
                    live.Add(new Vector2I(i, j));
                }
            }
        }
        map.Clear();
        foreach (var cell in live)
        {
            TileId = audioMap.Contains(cell) ? new Vector2I(1, 0) : new Vector2I(0, 0);
            map.SetCell(cell, 0, TileId);
        }

    }
    public int GetNeighbors(int x, int y)
    {
        int n = 0;
        for (int i = x - 1; i < x + 2; i++)
        {
            for (int j = y - 1; j < y + 2; j++)
            {
                if (!(i == x && j == y))
                {
                    if (map.GetCellSourceId(new Vector2I(i, j)) > -1)
                    {
                        n++;
                    }
                }
            }
        }
        return n;
    }
    public void ValueChanged(float Value)
    {
        if (Value == 0)
        {
            timer.Stop();
        }
        else if (timer.IsStopped() && Value > 0)
        {
            timer.Start();
        }
        timer.WaitTime = Value > 0 ? Value : 0.1;
    }

    public void XScrollValueChanged(float value)
    {
        xStep = (int)value;
        timer.Stop();
        slider.Value = 0;
        audioMap = GenerateNoteNodes(xStep, yStep);
        map.Clear();
        for (int i = 0; i < audioMap.Count; i++)
        {
            map.SetCell(audioMap[i], 0, new Vector2I(1, 0));
        }
    }

    public void YScrollValueChanged(float value)
    {
        yStep = (int)value;
        timer.Stop();
        slider.Value = 0;
        audioMap = GenerateNoteNodes(xStep, yStep);
        map.Clear();
        for (int i = 0; i < audioMap.Count; i++)
        {
            map.SetCell(audioMap[i], 0, new Vector2I(1, 0));
        }
    }
    public void ButtonPressed()
    {
        map.Clear();
    }
    public void ShufflePressed()
    {
        audioPaths = Shuffle(audioPaths);
        audioMap = GenerateNoteNodes(xStep, yStep);
    }
    public List<Vector2I> GenerateNoteNodes(int xStep, int yStep)
    {
        List<Vector2I> list = new List<Vector2I>();
        for (int i = 0; i < size.X; i += xStep)
        {
            for (int j = 0; j < size.Y; j += yStep)
            {
                list.Add(new Vector2I(i, j));
            }
        }
        return list;
    }
    public List<string> Shuffle(List<string> list)
    {
        var count = list.Count;
        var last = count - 1;
        for (int i = 0; i < last; ++i)
        {
            var r = new Random().Next(i, count);
            var tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
        return list;
    }
}
