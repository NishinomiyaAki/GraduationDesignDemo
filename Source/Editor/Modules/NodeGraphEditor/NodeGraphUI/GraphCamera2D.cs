using EditorUI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CrossEditor
{
    public class GraphCamera2D
    {
        struct Viewport
        {
            public float _WorldX;
            public float _WorldY;
            public float _Height;
            public float _AspectRatio;

            public void SaveToXml(Record record)
            {
                record.SetTypeString("Viewport");
                record.SetFloat("_WorldX", _WorldX);
                record.SetFloat("_WorldY", _WorldY);
                record.SetFloat("_Height", _Height);
                record.SetFloat("_AspectRatio", _AspectRatio);
            }

            public void LoadFromXml(Record record)
            {
                _WorldX = record.GetFloat("_WorldX");
                _WorldY = record.GetFloat("_WorldY");
                _Height = record.GetFloat("_Height");
                _AspectRatio = record.GetFloat("_AspectRatio");
            }
        }

        Viewport _WorldViewport;
        Viewport _ScreenViewport;

        // For zoom
        int MinZoom = -10;
        int MaxZoom = +5;
        int ZoomCount = 0;

        public float ScreenX
        {
            get => _ScreenViewport._WorldX;
            set => _ScreenViewport._WorldX = value;
        }

        public float ScreenY
        {
            get => _ScreenViewport._WorldY;
            set => _ScreenViewport._WorldY = value;
        }

        public float ScreenHeight
        {
            get => _ScreenViewport._Height;
            set
            {
                if (value > 0.0f)
                {
                    float scale = value / _ScreenViewport._Height;
                    _WorldViewport._Height *= scale;
                    _ScreenViewport._Height = value;
                }
            }
        }

        public float AspectRatio
        {
            get => _ScreenViewport._AspectRatio;
            set
            {
                if (value > 0.0f)
                {
                    _ScreenViewport._AspectRatio = value;
                    _WorldViewport._AspectRatio = value;
                }
            }
        }

        public float WorldX
        {
            get => _WorldViewport._WorldX;
            set => _WorldViewport._WorldX = value;
        }

        public float WorldY
        {
            get => _WorldViewport._WorldY;
            set => _WorldViewport._WorldY = value;
        }

        public float WorldHeight
        {
            get => _WorldViewport._Height;
            set
            {
                if (value > 0.0f)
                {
                    _WorldViewport._Height = value;
                }
            }
        }

        public string GetZoomText()
        {
            string Text = "Zoom ";

            if (ZoomCount == 0)
            {
                Text += "1:1";
            }
            else
            {
                Text += ZoomCount.ToString();
            }

            return Text;
        }

        public float GetZoomRatio()
        {
            return ScreenHeight / WorldHeight;
        }

        public GraphCamera2D()
        {
            _ScreenViewport = new Viewport { _WorldX = 0, _WorldY = 0, _AspectRatio = 1, _Height = 10 };
            _WorldViewport = new Viewport { _WorldX = 0, _WorldY = 0, _AspectRatio = 1, _Height = 10 };
        }

        public void Initialize(int inBaseX, int inBaseY, int inHeight, int inWidth)
        {
            ScreenX = inBaseX;
            ScreenY = inBaseY;
            ScreenHeight = inHeight;
            AspectRatio = inWidth * 1.0f / inHeight;
        }

        public void SetZoomRange(int Min, int Max)
        {
            MinZoom = Min;
            MaxZoom = Max;
        }

        public void Zoom(int WorldPinX, int WorldPinY, float scale)
        {
            WorldX = (WorldX - WorldPinX) * scale + WorldPinX;
            WorldY = (WorldY - WorldPinY) * scale + WorldPinY;
            WorldHeight = WorldHeight * scale;
        }

        public void ZoomBigger(int PinX, int PinY)
        {
            if(ZoomCount < MaxZoom)
            {
                Zoom(PinX, PinY, 0.8f);
                ZoomCount++;
            }
        }

        public void ZoomSmaller(int PinX, int PinY)
        {
            if (ZoomCount > MinZoom)
            {
                Zoom(PinX, PinY, 1.25f);
                ZoomCount--;
            }
        }

        public void WorldToScreen(int inWorldX, int inWorldY, ref int outScreenX, ref int outScreenY)
        {
            outScreenX = (int)(ScreenHeight * (inWorldX - WorldX) / WorldHeight + ScreenX);
            outScreenY = (int)(ScreenHeight * (inWorldY - WorldY) / WorldHeight + ScreenY);
        }

        public void ScreenToWorld(int inScreenX, int inScreenY,ref int outWorldX, ref int outWorldY)
        {
            outWorldX = (int)(WorldHeight * (inScreenX - ScreenX) / ScreenHeight + WorldX);
            outWorldY = (int)(WorldHeight * (inScreenY - ScreenY) / ScreenHeight + WorldY);
        }

        public void MoveX(float DeltaX)
        {
            WorldX += DeltaX * GetZoomRatio();
        }

        public void MoveY(float DeltaY)
        {
            WorldY += DeltaY * GetZoomRatio();
        }

        public void Move(float DeltaX, float DeltaY)
        {
            MoveX(DeltaX);
            MoveY(DeltaY);
        }

        public void SaveToXml(Record RecordCamera)
        {
            RecordCamera.SetTypeString("Camera");

            var recordWorld = RecordCamera.AddChild();
            _WorldViewport.SaveToXml(recordWorld);
            var recordScreen = RecordCamera.AddChild();
            _ScreenViewport.SaveToXml(recordScreen);

            RecordCamera.SetInt("MaxZoom", MaxZoom);
            RecordCamera.SetInt("MinZoom", MinZoom);
            RecordCamera.SetInt("ZoomCount", ZoomCount);
        }

        public void LoadFromXml(Record RecordCamera)
        {
            Record recordWorld = RecordCamera.GetChild(0);
            if (recordWorld != null)
            {
                _WorldViewport.LoadFromXml(recordWorld);
            }
            Record recordScreen = RecordCamera.GetChild(1);
            if (recordScreen != null)
            {
                _ScreenViewport.LoadFromXml(recordScreen);
            }

            MaxZoom = RecordCamera.GetInt("MaxZoom");
            MinZoom = RecordCamera.GetInt("MinZoom");
            ZoomCount = RecordCamera.GetInt("ZoomCount");
        }
    }
}
