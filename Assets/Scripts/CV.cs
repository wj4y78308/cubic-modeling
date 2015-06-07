using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CV {

	static int XY_SHIFT = 16, XY_ONE = 1 << XY_SHIFT;
	static int INT_MIN = int.MinValue, INT_MAX = int.MaxValue;

	public class PolyEdge {
		public int y0, y1;
		public int x, dx;
		public PolyEdge next;
	}

	public class Size {
		public int height, width;
		public Size (Texture2D tex){
			height = tex.height;
			width = tex.width;
		}
	}

	public static bool Equal (Texture2D tex1, Texture2D tex2) {
		Color[] color1 = tex1.GetPixels ();
		Color[] color2 = tex2.GetPixels ();

		for (int i=0; i<color1.Length; i++) {
			if( color1[i]!=color2[i]){
				return false;
			}
		}
		return true;
	}

	public static void FillPoly (Texture2D img, List<Vector2> pts, Color color) {
		List<PolyEdge> edges = new List<PolyEdge>();

		CollectPolyEdges(img, pts, pts.Count, edges, color);

		FillEdgeCollection(img, edges, color);
	}

	static void CollectPolyEdges (Texture2D img, List<Vector2> v, int count, List<PolyEdge> edges, Color color) {
		int i;
		Vector2 pt0 = v[count-1], pt1;
		pt0.x = (int)pt0.x << XY_SHIFT;
		//pt0.y = (pt0.y + delta) >> shift;
		
		for( i = 0; i < count; i++, pt0 = pt1 )
		{
			//Vector2 t0, t1;
			PolyEdge edge = new PolyEdge();
			
			pt1 = v[i];
			pt1.x = (int)pt1.x << XY_SHIFT;
			//pt1.y = pt1.y >> shift;

			/*t0.y = pt0.y; t1.y = pt1.y;
			t0.x = ((int)pt0.x + (XY_ONE >> 1)) >> XY_SHIFT;
			t1.x = ((int)pt1.x + (XY_ONE >> 1)) >> XY_SHIFT;*/
			//Line(img, t0, t1, color);
			
			if( pt0.y == pt1.y )
				continue;
			
			if( pt0.y < pt1.y )
			{
				edge.y0 = (int)pt0.y;
				edge.y1 = (int)pt1.y;
				edge.x = (int)pt0.x;
			}
			else
			{
				edge.y0 = (int)pt1.y;
				edge.y1 = (int)pt0.y;
				edge.x = (int)pt1.x;
			}
			edge.dx = (int)((pt1.x - pt0.x) / (pt1.y - pt0.y));
			edges.Add(edge);
		}
	}

	static void FillEdgeCollection( Texture2D img, List<PolyEdge> edges, Color color )
	{
		PolyEdge tmp = new PolyEdge();
		int i, y, total = edges.Count;
		Size size = new Size (img);
		PolyEdge e = new PolyEdge();
		int y_max = INT_MIN, x_max = INT_MIN, y_min = INT_MAX, x_min = INT_MAX;
		
		if( total < 2 )
			return;
		
		for( i = 0; i < total; i++ )
		{
			PolyEdge e1 = edges[i];
			// Determine x-coordinate of the end of the edge.
			// (This is not necessary x-coordinate of any vertex in the array.)
			int x1 = e1.x + (e1.y1 - e1.y0) * e1.dx;
			y_min = Mathf.Min( y_min, e1.y0 );
			y_max = Mathf.Max( y_max, e1.y1 );
			x_min = Mathf.Min( x_min, e1.x );
			x_max = Mathf.Max( x_max, e1.x );
			x_min = Mathf.Min( x_min, x1 );
			x_max = Mathf.Max( x_max, x1 );
		}
		
		if( y_max < 0 || y_min >= size.height || x_max < 0 || x_min >= (size.width<<XY_SHIFT) )
			return;

		edges.Sort ((a,b) => { return CmpEdges(a,b)? -1 : 1 ;} );
		
		// start drawing
		tmp.y0 = INT_MAX;
		edges.Add(tmp); // after this point we do not add
						// any elements to edges, thus we can use pointers
		i = 0;
		tmp.next = null;
		e = edges[i];
		y_max = Mathf.Min( y_max, size.height );

		for( y = e.y0; y < y_max; y++ )
		{
			PolyEdge last, prelast, keep_prelast;
			bool sort_flag = false;
			bool draw = false;
			bool clipline = y<0;
			
			prelast = tmp;
			last = tmp.next;
			while( last!=null || e.y0 == y )
			{
				if( last!=null && last.y1 == y )
				{
					// exclude edge if y reachs its lower point
					prelast.next = last.next;
					last = last.next;
					continue;
				}
				keep_prelast = prelast;
				if( last!=null && (e.y0 > y || last.x < e.x) )
				{
					// go to the next edge in active list
					prelast = last;
					last = last.next;
				}
				else if( i < total )
				{
					// insert new edge into active list if y reachs its upper point
					prelast.next = e;
					e.next = last;
					prelast = e;
					e = edges[++i];
				}
				else
					break;

				if( draw )
				{
					if( !clipline )
					{
						// convert x's from fixed-point to image coordinates;
						int x1 = keep_prelast.x;
						int x2 = prelast.x;

						if( x1 > x2 )
						{
							int t = x1;
							
							x1 = x2;
							x2 = t;
						}

						x1 = (x1 + XY_ONE - 1) >> XY_SHIFT;
						x2 = x2 >> XY_SHIFT;
						
						// clip and draw the line
						if( x1 < size.width && x2 >= 0 )
						{
							if( x1 < 0 )
								x1 = 0;
							if( x2 >= size.width )
								x2 = size.width - 1;

							for (int x = x1; x <= x2; x++) {
								img.SetPixel(x,y,color);
							}
						}
					}
					keep_prelast.x += keep_prelast.dx;
					prelast.x += prelast.dx;
				}
				draw = !draw;
			}
			
			// sort edges (using bubble sort)
			keep_prelast = null;
			
			do
			{
				prelast = tmp;
				last = tmp.next;
				
				while( last != keep_prelast && last.next != null )
				{
					PolyEdge te = last.next;
					
					// swap edges
					if( last.x > te.x )
					{
						prelast.next = te;
						last.next = te.next;
						te.next = last;
						prelast = te;
						sort_flag = true;
					}
					else
					{
						prelast = last;
						last = te;
					}
				}
				keep_prelast = prelast;
			}
			while( sort_flag && keep_prelast != tmp.next && keep_prelast != tmp );
		}
	}

	static public void MeansDenoising (Texture2D img) {
		List<Vector2> noise = new List<Vector2>();
		for(int y=0; y<img.height; y++){
			for(int x=0; x<img.width; x++){
				if(img.GetPixel(x,y)==Color.clear) continue;
				int a=0,b=0;
				if(x-1>=0 && y-1>=0) {
					if(img.GetPixel(x-1,y-1)!=Color.clear) a++;
					b++;
				}
				if(x-1>=0) {
					if(img.GetPixel(x-1,y)!=Color.clear) a++;
					b++;
				}
				if(y-1>=0) {
					if(img.GetPixel(x,y-1)!=Color.clear) a++;
					b++;
				}
				if(x+1<img.width && y-1>=0) {
					if(img.GetPixel(x+1,y-1)!=Color.clear) a++;
					b++;
				}
				if(x-1>=0 && y+1<img.height) {
					if(img.GetPixel(x-1,y+1)!=Color.clear) a++;
					b++;
				}
				if(x+1<img.width) {
					if(img.GetPixel(x+1,y)!=Color.clear) a++;
					b++;
				}
				if(y+1<img.height) {
					if(img.GetPixel(x,y+1)!=Color.clear) a++;
					b++;
				}
				if(x+1<img.width && y+1<img.height) {
					if(img.GetPixel(x+1,y+1)!=Color.clear) a++;
					b++;
				}
				if(a/b<0.3) noise.Add(new Vector2(x,y));
			}
		}
		foreach (Vector2 n in noise) {
			img.SetPixel((int)n.x,(int)n.y,Color.clear);
		}
	}

	static bool CmpEdges (PolyEdge e1, PolyEdge e2) {
		return e1.y0 != e2.y0 ? e1.y0 < e2.y0 : e1.x != e2.x ? e1.x < e2.x : e1.dx < e2.dx;
	}

	public static void ResizePointArray (List<Vector2> pointArray, float scale) {
		for (int i=0; i<pointArray.Count; i++) 
			pointArray [i] = new Vector2((int)(pointArray [i].x * scale),(int)(pointArray [i].y * scale));
	}


	public static void Resize (Texture2D tex, int newWidth, int newHeight) {
		Color[] texColors, newColors;
		int w, w2;
		float ratioX, ratioY; 

		texColors = tex.GetPixels();
		newColors = new Color[newWidth * newHeight];

		ratioX = ((float)tex.width) / newWidth;
		ratioY = ((float)tex.height) / newHeight;

		w = tex.width;
		w2 = newWidth;

		for (int y = 0; y < newHeight; y++)
		{
			var thisY = (int)(ratioY * y) * w;
			var yw = y * w2;
			for (var x = 0; x < w2; x++) {
				newColors[yw + x] = texColors[(int)(thisY + ratioX*x)];
			}
		}
		
		tex.Resize(newWidth, newHeight);
		tex.SetPixels(newColors);
	}

	public static Vector2 GetMidPoint (Texture2D tex) {
		int minX, minY, maxX, maxY;
		minX = minY = INT_MAX;
		maxX = maxY = INT_MIN;

		for (int y=0; y<tex.height; y++) {
			for (int x=0; x<tex.width; x++) {
				if (tex.GetPixel(x,y) != Color.clear) {
					if (x < minX) minX = x;
					if (y < minY) minY = y;
					if (x > maxX) maxX = x;
					if (y > maxY) maxY = y;
				}
			}
		}
		if (minX == INT_MAX) {
			minX = maxX = (int)(tex.width/2);
			minY = maxY = (int)(tex.height/2);
		}

		return new Vector2 ((minX + maxX) / 2, (minY + maxY) / 2);
	} 

	public static Color HSVToRGB(float H, float S, float V)
	{
		if (S == 0f)
			return new Color(V,V,V);
		else if (V == 0f)
			return Color.black;
		else
		{
			Color col = Color.black;
			float Hval = H * 6f;
			int sel = Mathf.FloorToInt(Hval);
			float mod = Hval - sel;
			float v1 = V * (1f - S);
			float v2 = V * (1f - S * mod);
			float v3 = V * (1f - S * (1f - mod));
			switch (sel + 1)
			{
			case 0:
				col.r = V;
				col.g = v1;
				col.b = v2;
				break;
			case 1:
				col.r = V;
				col.g = v3;
				col.b = v1;
				break;
			case 2:
				col.r = v2;
				col.g = V;
				col.b = v1;
				break;
			case 3:
				col.r = v1;
				col.g = V;
				col.b = v3;
				break;
			case 4:
				col.r = v1;
				col.g = v2;
				col.b = V;
				break;
			case 5:
				col.r = v3;
				col.g = v1;
				col.b = V;
				break;
			case 6:
				col.r = V;
				col.g = v1;
				col.b = v2;
				break;
			case 7:
				col.r = V;
				col.g = v3;
				col.b = v1;
				break;
			}
			col.r = Mathf.Clamp(col.r, 0f, 1f);
			col.g = Mathf.Clamp(col.g, 0f, 1f);
			col.b = Mathf.Clamp(col.b, 0f, 1f);
			return col;
		}
	}
}
