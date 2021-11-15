using System;
using Lab2_Types;
using System.Collections;
using System.Numerics;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Lab2_Main 
{
    class Program
    {
        static int Main()
        {
            Method1();
            //Method2();
            return 0;
        }

        static int Method1()
        {
            
            Fv2Complex func = Func.func1;//x * x + y * y
            Vector2 step = new (0.1F, 0.2F);
            string format = "F2";
            
            V2DataArray x_array = new ("Data Array ", DateTime.Now, 3, 2, step, func);
            
            x_array.SaveAsText("x_array.txt");
            V2DataArray y_array = new ("Data Array ", DateTime.Now);
            y_array.LoadAsText("x_array.txt", ref y_array);
            
            Console.WriteLine("Original Array");
            Console.WriteLine(x_array.ToLongString(format));
            Console.WriteLine("Read/Load Array");
            Console.WriteLine(y_array.ToLongString(format));

            
            V2DataList x_list = (V2DataList)x_array;
            x_list.SaveBinary("x_list.bin");
            V2DataList y_list = new ("Data_list ", DateTime.Now);
            y_list.LoadBinary("x_list.bin", ref y_list);

            Console.WriteLine("Original List");
            Console.WriteLine(x_list.ToLongString(format));
            Console.WriteLine("Read/Load List");
            Console.WriteLine(y_list.ToLongString(format));
            return 0;
        }

        static int Method2()
        {
            Fv2Complex func = Func.func1;//x * x + y * y
            Vector2 step = new (0.1F, 0.2F);
            string format = "F2";
            V2MainCollection x_collect = new();


            V2DataList x_list = new ("Data_list ", DateTime.Now);
            x_list.AddDefaults(4, func);
            V2DataList y_list = new ("Data_list ", DateTime.Now);//empty for check

            V2DataArray x_array = new ("Data Array ", DateTime.Now, 3, 2, step, func);
            V2DataArray y_array = new ("Data Array ", DateTime.Now);//empty for check

            x_collect.Add(x_list);
            x_collect.Add(y_list);
            x_collect.Add(x_array);
            x_collect.Add(y_array);

            Console.WriteLine("Original main Collection:");
            Console.WriteLine(x_collect.ToLongString(format));

            //=============================Check LINQ Max_In_V2MainCollection===============================
            Console.WriteLine("Max in V2MainCollection:");
            DataItem max_in_col = (DataItem)x_collect.Max_In_V2MainCollection;
            Console.WriteLine(max_in_col.ToLongString("F2"));

            //================================Check LINQ Only_in_V2DataList=================================
            Console.WriteLine("\nPoints from DataList only:");
            var col = x_collect.Only_in_V2DataList;
            foreach (Vector2 v2 in col)
            {
                Console.WriteLine(v2);
            }
            //==============================Check LINQ Number_Of_Measurements===============================
            Console.WriteLine("\n");
            var msr = x_collect.Number_Of_Measurements;
            foreach (IGrouping<int, V2Data> cnt in msr)
            {
                Console.WriteLine("Number of points: {0}", cnt.Key);
                foreach (var names in cnt)
                {
                    Console.WriteLine(names);
                }
            }
            return 0;
        }
    }
}







namespace Lab2_Types
{
    struct DataItem
    {
        public Vector2 crdnts { get; set; }
        public Complex field_val { get; set; }
        public DataItem(Vector2 z, Complex val)
        {
            field_val = val;
            crdnts = z;
        }
        public string ToLongString(string format)
        {
            return String.Concat("Type: DataItem   X: ", crdnts.X.ToString(format),"  Y: ", crdnts.Y.ToString(format), "  Field_val: ",
                    field_val.ToString(format), "  Magnitude: ", field_val.Magnitude.ToString(format));
        }
        public override string ToString()
        {
            return base.ToString();
        }
    }





    //===========================================V2Data=============================================
    abstract class V2Data: IEnumerable<DataItem>
    {
        public string ident { get; protected set; }
        public DateTime date { get; protected set; }
        public V2Data(string s, DateTime t)
        {
            ident = s;
            date = t;
        }
        public abstract int Count { get; }
        public abstract float MinDistance { get; }
        public abstract string ToLongString(string format);
        public override string ToString()
        {
            return String.Concat(ident, date.ToString());
        }
        public abstract IEnumerator<DataItem> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        public abstract Complex max_val { get; }
    }



    //=========================================V2DataList===========================================
    class V2DataList : V2Data, IEnumerable<DataItem>
    {
        public List<DataItem> data_list { get; }
        public V2DataList(string s, DateTime t) : base(s, t)
        {
            data_list = new List<DataItem>();
        }
        public bool Add(DataItem newItem)
        {
            foreach (DataItem i in data_list)
            {
                if (i.crdnts == newItem.crdnts)
                {
                    return false;
                }
            }
            data_list.Add(newItem);
            return true;
        }

        public int AddDefaults(int nItems, Fv2Complex F)
        {
            int n = 0;//number of added values
            for (int i = 0; i < nItems; i++)
            {
                Random x = new();
                double x_cur = Convert.ToDouble(x.Next(-100, 100) / 10.0);
                double y_cur = Convert.ToDouble(x.Next(-100, 100) / 10.0);
                Vector2 coord = new ((float)x_cur, (float)y_cur);
                Complex f_v = F(coord);
                DataItem data_new = new (coord, f_v);
                if (this.Add(data_new))
                {
                    n++;
                }
            }
            return n;
        }
        public override int Count
        {
            get { return this.data_list.Count; }
        }
        public override float MinDistance
        {
            get
            {
                if (data_list.Count <= 1) return 0;
                float min = Vector2.Distance(data_list[0].crdnts, data_list[1].crdnts);
                for (int i = 0; i < data_list.Count - 1; i++)
                {
                    for (int j = i + 1; j < data_list.Count; j++)
                    {
                        float cur_dist = Vector2.Distance(data_list[i].crdnts, data_list[j].crdnts);
                        if (cur_dist < min)
                        {
                            min = cur_dist;
                        }
                    }
                }
                return min;
            }
        }

        public override string ToString()
        {
            return String.Concat("V2DataList ", base.ident, base.date, " Number " +
                "of elements: ", data_list.Count, "\n");
        }
        public override string ToLongString(string format)
        {
            string res = String.Concat(this.ToString());
            foreach (DataItem i in data_list)
            {
                res = String.Concat(res, "Coordinates: ", i.crdnts.ToString(format), " Field value: ",
                        i.field_val.ToString(format), " Field Abs value: ",
                        i.field_val.Magnitude.ToString(format), "\n");
            }
            return res;
        }


        public override IEnumerator<DataItem> GetEnumerator()
        {
            return data_list.GetEnumerator();//List<MyObject> has a built-in GetEnumerator
        }


        public bool SaveBinary(string filename)
        {
            //save V2dataList in filename file
            FileStream fs = null;
            try
            {
                fs = File.Open(filename, FileMode.Create);
                BinaryWriter writer = new (fs);
                writer.Write(data_list.Count);
                foreach (DataItem item in data_list)
                {
                    writer.Write(item.crdnts.X);
                    writer.Write(item.crdnts.Y);
                    writer.Write(item.field_val.Real);
                    writer.Write(item.field_val.Imaginary);
                }
                writer.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                if (fs != null) fs.Close();
            }
            return true;
        }

        public bool LoadBinary(string filename, ref V2DataList v2)
        {
            //save V2dataList to v2 from filename file
            FileStream fs = null;
            try
            {
                fs = File.Open(filename, FileMode.Open);
                BinaryReader reader = new (fs);
                float cord_x = 0;
                float cord_y = 0;
                double f_val_re = 0;
                double f_val_im = 0;
                string idnt = "Data List";
                DateTime t = DateTime.Now;
                v2 = new V2DataList(idnt, t);
                int cnt = reader.ReadInt32();
                for (int i = 0; i < cnt; i++)
                {
                    cord_x = reader.ReadSingle();
                    cord_y = reader.ReadSingle();
                    f_val_re = reader.ReadDouble();
                    f_val_im = reader.ReadDouble();
                    Vector2 coord = new (cord_x, cord_y);
                    Complex f_v = new (f_val_re, f_val_im);
                    DataItem tmp = new (coord, f_v);
                    v2.Add(tmp);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                if (fs != null) fs.Close();
            }
            return true;
        }

        public override Complex max_val 
        {
            get
            {
                double max_abs = 0;
                Complex max_cmplx = new(0, 0);
                foreach (DataItem x in data_list)
                {
                    if (Complex.Abs(x.field_val) > max_abs)
                    {
                        max_abs = Complex.Abs(x.field_val);
                        max_cmplx = x.field_val;
                    }
                }
                return max_cmplx;
            } 
        }
    }



    //=========================================V2DataArray==========================================
    class V2DataArray : V2Data, IEnumerable<DataItem>
    {
        public int X_num_nods { get; private set; }
        public int Y_num_nods { get; private set; }
        public Vector2 step { get; private set; }
        public Complex[,] field_val { get; private set; }
        public V2DataArray(string s, DateTime t) : base(s, t)
        {
            field_val = new Complex[0, 0];
        }
        public V2DataArray(string s1, DateTime t, int x, int y, Vector2 s,
                Fv2Complex f) : base(s1, t)//f (Vector2 v2)
        {
            X_num_nods = x;
            Y_num_nods = y;
            step = s;
            field_val = new Complex[x, y];
            for (int i = 0; i < X_num_nods; i++)
            {
                double x_cur = i * step.X;//follow ox from 0 with step.x step
                for (int j = 0; j < Y_num_nods; j++)
                {
                    double y_cur = j * step.Y;//follow oy from 0 with step.y step
                    Vector2 v2 = new ((float)x_cur, (float)y_cur);
                    field_val[i, j] = f(v2);
                }
            }
        }
        public override int Count
        {
            get
            {
                return X_num_nods * Y_num_nods;
            }
        }
        public override float MinDistance
        {
            get
            {
                return (step.X < step.Y ? step.X : step.Y);
            }
        }

        public override string ToString()
        {
            return String.Concat("V2DataArray ", base.ToString(), " Size: ", X_num_nods.ToString(),
                    " on ", Y_num_nods.ToString(), ", step in x and y: ", step.ToString() + "\n");
        }
        public override string ToLongString(string format)
        {
            string res = String.Concat(this.ToString());
            for (int i = 0; i < X_num_nods; i++)
            {
                double x_cur = i * step.X;
                for (int j = 0; j < Y_num_nods; j++)
                {
                    double y_cur = j * step.Y;
                    res = res + "Coordinates: x = " + x_cur.ToString(format) + ", y = " + y_cur.ToString(format) +
                            ", Field value =  " + field_val[i, j].ToString(format) + ", Abs Field value = "
                            + field_val[i, j].Magnitude.ToString(format) + "\n";

                }
            }
            return res;
        }

        public static explicit operator V2DataList(V2DataArray a)
        {
            V2DataList res = new (a.ident, a.date);
            for (int i = 0; i < a.X_num_nods; i++)
            {
                float x_cur = i * a.step.X;
                for (int j = 0; j < a.Y_num_nods; j++)
                {
                    float y_cur = j * a.step.Y;
                    Vector2 cord = new (x_cur, y_cur);
                    DataItem tmp = new (cord, a.field_val[i, j]);
                    res.Add(tmp);
                }
            }
            return res;
        }

        public override IEnumerator<DataItem> GetEnumerator()
        {
            return (IEnumerator<DataItem>) new V2DataArray_Enumerator(this);
        }

        public bool SaveAsText(string filename)
        {
            //save V2dataArray in filename file
            FileStream fs = null;
            try
            {
                fs = new FileStream(filename, FileMode.OpenOrCreate);
                StreamWriter writer = new (fs);
                writer.WriteLine(step.X);
                writer.WriteLine(step.Y);
                writer.WriteLine(X_num_nods);
                writer.WriteLine(Y_num_nods);
                for (int i = 0; i < X_num_nods; i++)
                {
                    for (int j = 0; j < Y_num_nods; j++)
                    {
                        writer.WriteLine(field_val[i, j].Real);
                        writer.WriteLine(field_val[i, j].Imaginary);
                    }
                }
                writer.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                if (fs != null) fs.Close();
            }
            return true;
        }

        public bool LoadAsText(string filename, ref V2DataArray v2)
        {
            //save V2dataArray to v2 from filename file
            FileStream fs = null;
            try
            {
                fs = new FileStream(filename, FileMode.OpenOrCreate);
                StreamReader reader = new (fs);
                float step_x = (float)Convert.ToDouble(reader.ReadLine());
                float step_y = (float)Convert.ToDouble(reader.ReadLine());
                Vector2 step = new (step_x, step_y);
                int x_nods = Convert.ToInt32(reader.ReadLine());
                int y_nods = Convert.ToInt32(reader.ReadLine());
                v2 = new V2DataArray(v2.ident, v2.date, x_nods, y_nods, step, Func.func1);
                for (int i = 0; i < x_nods; i++)
                {
                    for (int j = 0; j < y_nods; j++)
                    {
                        double r_c = Convert.ToDouble(reader.ReadLine());
                        double im_c = Convert.ToDouble(reader.ReadLine());
                        v2.field_val[i, j] = new Complex(r_c, im_c);
                    }
                }
                reader.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                if (fs != null) fs.Close();
            }
            return true;
        }

        public override Complex max_val
        {
            get
            {
                double max_abs = 0;
                Complex max_cmplx = new(0, 0);
                for (int i = 0; i < X_num_nods; i++)
                {
                    for (int j = 0; j < Y_num_nods; j++)
                    {
                        if (Complex.Abs(field_val[i, j]) > max_abs)
                        {
                            max_abs = Complex.Abs(field_val[i, j]);
                            max_cmplx = field_val[i, j];
                        }
                    }
                }
                return max_cmplx;
            }
        }

    }



    //===================================V2DataArray_Enumerator=====================================
    class V2DataArray_Enumerator : IEnumerator<DataItem>
    {
        private int x_cur_nod = 0;
        private int y_cur_nod = -1;
        private V2DataArray arr;
        private int flag = 0;//out of the array = 1
        private DataItem cur_data_item;

        public V2DataArray_Enumerator(V2DataArray x)
        {
            x_cur_nod = 0;
            y_cur_nod = -1;
            arr = x;
        }
        public DataItem Current
        {
            get
            {
                if (arr == null || x_cur_nod == -1 || flag == 1)
                {
                    Console.WriteLine("Current error");
                    throw new InvalidOperationException();
                }
                return cur_data_item;
            }
        }
        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public bool MoveNext()
        {
            if (arr.X_num_nods == 0 || arr.Y_num_nods == 0)
            {
                flag = 1;
                return false;//empty array
            }
            if (y_cur_nod < arr.Y_num_nods - 1)
            {
                y_cur_nod += 1;
                float x_cur = x_cur_nod * arr.step.X;
                float y_cur = y_cur_nod * arr.step.Y;
                Vector2 coord = new(x_cur, y_cur);
                cur_data_item = new(coord, arr.field_val[x_cur_nod, y_cur_nod]);
                return true;
            }
            else
            {
                if (x_cur_nod < arr.X_num_nods - 1)
                {
                    x_cur_nod += 1;
                    y_cur_nod = 0;
                    float x_cur = x_cur_nod * arr.step.X;
                    float y_cur = y_cur_nod * arr.step.Y;
                    Vector2 coord = new(x_cur, y_cur);
                    cur_data_item = new(coord, arr.field_val[x_cur_nod, y_cur_nod]);
                    return true;
                }
                else
                {
                    flag = 1;//out of array
                    return false;
                }
            }
        }

        public void Reset()
        {
            x_cur_nod = 0;
            y_cur_nod = -1;
            flag = 0;
        }

        void IDisposable.Dispose() 
        {
            return;
        }
    }



    //======================================V2MainCollection========================================
    class V2MainCollection
    {
        private List<V2Data> v2_data_list = new ();

        public int Count
        {
            get { return v2_data_list.Count; }
        }
        public V2Data this[int i]
        {
            get { return v2_data_list[i]; }
        }
        public bool Contains(string ID)
        {
            Console.WriteLine(v2_data_list[1]);
            bool res = false;
            foreach (V2Data i in v2_data_list)
            {
                if (i.ident == ID)
                {
                    res = true;
                    break;
                }
            }
            return res;
        }
        public bool Add(V2Data v2data)
        {
            bool res = false;
            if (!v2_data_list.Contains(v2data) || v2_data_list.Count == 0)
            {
                v2_data_list.Add(v2data);
                res = true;
            }
            return res;
        }
        public string ToLongString(string format)
        {
            string res = "V2MainCollection:" + "\n";
            foreach (V2Data i in v2_data_list)
            {
                res += i.ToLongString(format);
            }
            return res;
        }
        public override string ToString()
        {
            string res = "";
            foreach (V2Data i in v2_data_list)
            {
                res += i.ToString();
            }
            return res;
        }




        public DataItem? Max_In_V2MainCollection
        {
            get 
            {
                if (v2_data_list.Count == 0)
                {
                    return null;
                }
                var cnt_each_elmnt = from item1 in v2_data_list
                        where item1.Count != 0
                        select item1;
                if (!cnt_each_elmnt.Any())
                {
                    return null;
                }
                var res = from item1 in v2_data_list
                        from item2 in item1
                        where (item2.field_val.Real.Equals(item1.max_val.Real) &&
                        item2.field_val.Imaginary.Equals(item1.max_val.Imaginary))
                        select item2;
                DataItem res_item = new(res.FirstOrDefault().crdnts, res.FirstOrDefault().field_val);
                return res_item;
            }
        }

        public IEnumerable<Vector2> Only_in_V2DataList
        {
            get
            {
                if (v2_data_list.Count == 0)
                {
                    return null;
                }
                var cnt_each_elmnt = from item1 in v2_data_list
                        where item1.Count != 0
                        select item1;
                if (!cnt_each_elmnt.Any())
                {
                    return null;
                }

                var points_in_list = from item1 in v2_data_list 
                        from item2 in item1
                        where item1 is V2DataList
                        select item2.crdnts;//all points in Lists
                
                var points_in_array = from item1 in v2_data_list
                        from item2 in item1
                        where item1 is V2DataArray
                        select item2.crdnts;//all points in Arrays

                var res = points_in_list.Except(points_in_array);
                res = res.Distinct();//unique points
                if (res.Any())
                {
                    return res;
                }
                else
                {
                    return null;
                }

            }
        }

        public IEnumerable<IGrouping<int, V2Data>> Number_Of_Measurements
        {
            get
            {
                if (v2_data_list.Count == 0)
                {
                    return null;
                }
                var cnt_each_elmnt = from item1 in v2_data_list
                                     where item1.Count != 0
                                     select item1;
                if (!cnt_each_elmnt.Any())
                {
                    return null;
                }

                var res = from item1 in v2_data_list
                        group item1 by item1.Count into gr
                        select gr;
                return res;
            }
        }

    }



    //==========================================Delegate============================================
    public delegate Complex Fv2Complex(Vector2 v2);



    //==========================================Function============================================
    public static class Func
    {
        public static Complex func1(Vector2 v)
        {
            Complex p = new (0.0, 1.0);
            Complex res = p * (v.X * v.X + v.Y * v.Y);//i(x*x + y*y)
            return res;
        }
    }
}
