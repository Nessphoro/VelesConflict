using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace VelesConflict.Shared
{

    public class Cell
    {
        public Vector2 Position;
        public Vector2 DrawPosition;
        public Color Color;
        public event Action<Cell> OnClick;
        public Cell[] Neighboors;
        public object Tag { get; set; }

        public Cell(Vector2 p)
        {
            Position = p;
            Neighboors = new Cell[6];
        }

        public void Click()
        {
            if (OnClick != null)
                OnClick(this);
        }
    }


    class HexFactory
    {

        List<Cell> list;

        public Cell[] GetHexFill(int CircleRadius, Vector2 Center)
        {
            list = new List<Cell>();
            list.Add(new Cell(new Vector2(512f, 512f)));
            list.Add(new Cell(new Vector2(512f, 400f)));
            list.Add(new Cell(new Vector2(608f, 456f)));
            list.Add(new Cell(new Vector2(608f, 568f)));
            list.Add(new Cell(new Vector2(512f, 624f)));
            list.Add(new Cell(new Vector2(416f, 568f)));
            list.Add(new Cell(new Vector2(416f, 456f)));
            list.Add(new Cell(new Vector2(512f, 288f)));
            list.Add(new Cell(new Vector2(608f, 344f)));
            list.Add(new Cell(new Vector2(416f, 344f)));
            list.Add(new Cell(new Vector2(704f, 400f)));
            list.Add(new Cell(new Vector2(704f, 512f)));
            list.Add(new Cell(new Vector2(704f, 624f)));
            list.Add(new Cell(new Vector2(608f, 680f)));
            list.Add(new Cell(new Vector2(512f, 736f)));
            list.Add(new Cell(new Vector2(416f, 680f)));
            list.Add(new Cell(new Vector2(320f, 624f)));
            list.Add(new Cell(new Vector2(320f, 512f)));
            list.Add(new Cell(new Vector2(320f, 400f)));
            list.Add(new Cell(new Vector2(512f, 176f)));
            list.Add(new Cell(new Vector2(608f, 232f)));
            list.Add(new Cell(new Vector2(416f, 232f)));
            list.Add(new Cell(new Vector2(704f, 288f)));
            list.Add(new Cell(new Vector2(320f, 288f)));
            list.Add(new Cell(new Vector2(800f, 344f)));
            list.Add(new Cell(new Vector2(800f, 456f)));
            list.Add(new Cell(new Vector2(800f, 568f)));
            list.Add(new Cell(new Vector2(800f, 680f)));
            list.Add(new Cell(new Vector2(704f, 736f)));
            list.Add(new Cell(new Vector2(608f, 792f)));
            list.Add(new Cell(new Vector2(512f, 848f)));
            list.Add(new Cell(new Vector2(416f, 792f)));
            list.Add(new Cell(new Vector2(320f, 736f)));
            list.Add(new Cell(new Vector2(224f, 680f)));
            list.Add(new Cell(new Vector2(224f, 568f)));
            list.Add(new Cell(new Vector2(224f, 456f)));
            list.Add(new Cell(new Vector2(224f, 344f)));



            foreach (Cell c in list)
            {
                c.Position.X += 32;
            }
            return list.ToArray();
        }
    }
}
