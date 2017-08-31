using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

/// <summary>
/// IT TAKES A LOT OF TIME TO DEBUG
/// To analyze FBX file(TO explain in another document)
/// </summary>
public class ParseBinary_2 : MonoBehaviour
{
    #region Lists
    //To store a hierarchy in the fbx file 
    List<int> ParentList;
    //for an array of integer or double (in binary)
    List<int> binaireList8;

    //Codes:    Values from 0 to 255 for Huffman Coding
    //          256 means the end of block
    //          if a value superior to 256, another value is read and these values are sandwiched by -1 and -2 
    //          ex. 25 17 127 -1(Start) 258 12 -2(end) 256(End of block)
    List<int> Codes, Doubles;
    #endregion


    // TO OUTPUT
    StreamWriter sw;

    /// <summary>
    /// Write debug1 under the name of ___.txt
    /// </summary>
    /// <param name="text"></param>
    /// <param name="IsAppend">This is currently not used. You can put any value you want. </param>
    private void writeText(string text, bool IsAppend)
    {
        //IsAppend is finally not used.
        sw.WriteLine(text);
        sw.Flush();
    }
    #region function and variables for debug
    private string Comment;
    private string debug1;
    private int depth;
    string _tab(int i)
    {
        string tab = "";
        for (int _i = 0; _i < i; _i++)
        {
            tab += "\t";
        }
        return tab;
    }
    #endregion
    #region private variables
    //Till how much data you want to analyze(if the total size is too big)
    private const int numBytes = 300000;
    //To pass data from a function to another
    private int curPos; private string BinaryString;
    //All fbx data 
    private byte[] rawData;
    //file name that we want to analyze
    private string fbxfile;
    //the size of fbx file
    private int fileLen; private int fbxVersion;
    #endregion
    // Use this for initialization
    void Start()
    {
        ParentList = new List<int>();
        depth = 0;
        //If you want to comment on the top of the file
        Comment = "[Comment]:Working on Custom Huffman \n";
        //fbxfile = "cube.fbx";
        //fbxfile = "ComPAC.fbx";
        fbxfile = "Sphere100.fbx";
        TextAsset textasset = Resources.Load(fbxfile) as TextAsset;
        rawData = textasset.bytes;
        fileLen = rawData.Length;
        debug1 = "";
        Debug.Log("file_size: " + rawData.Length);
        debug1 += Comment;

        string filename = Path.GetFileNameWithoutExtension(fbxfile);
        sw = new StreamWriter(Application.dataPath + "/Console/" + filename + "_ForCustomHuffman_s_" + "v401.txt", false);

        debugBytes(rawData);
        byte2Binairy(49);
        sw.Dispose();

    }

    #region JUST CONVERTING bytes to ASCII
    void debugByte2(byte[] bytes)
    {
        List<byte> list = new List<byte>();
        List<string> slist = new List<string>();
        for (int i = 0; i < fileLen; i++)
        {
            list.Add(bytes[i]);
            slist.Add(Byte2Str(bytes[i]));
            if (i % 16 == 15)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    debug1 += list[j].ToString("x2") + " ";
                }
                debug1 += "|";
                for (int k = 0; k < slist.Count; k++)
                {
                    debug1 += slist[k] + " ";
                }
                debug1 += "\n";
                list.Clear();
                slist.Clear();
            }
        }
        writeText(debug1, false);
    }
    #endregion

    #region THIS IS WHERE WE START

    void debugBytes(byte[] bytes)
    {
        int i = 0;
        debug1 += Comment;
        while (i < fileLen)
        {
            if (i < 27)
            {
                writeHeader(bytes, i);
            }
            else
            {
                i = writeContent(bytes, i);
            }
            i++;
        }
        print(debug1);
        writeText(debug1, false);
    }

    #endregion
    /// <summary>
    /// From i=20 to 23 
    /// TO write Header
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="i"></param>
    void writeHeader(byte[] bytes, int i)
    {

        if (i <= 20)
        {
            debug1 += Byte2Str(bytes[i]);
        }
        else if (21 == i)
        {
            debug1 += "0x1a";
        }
        else if (22 == i)
        {
            debug1 += "0x00\n";
        }
        else if (23 <= i && i <= 26)
        {
            if (i == 23)
            {
                fbxVersion = decimalVersion4(bytes, i);
                debug1 += "Version: " + fbxVersion + "\n\n";
            }
        }
    }

    void checkHierarchy(int offset)
    {
        if (ParentList.Count != 0)
        {
            if (ParentList.Find(n => n < offset) < offset)
            {
                int index = ParentList.FindIndex(n => n < offset);
                if (index >= 0)
                {
                    ParentList.RemoveAt(index);
                    debug1 += _tab(depth) + "}" + "\n";
                    depth--;
                    debug1 += _tab(depth) + "---End of Nested list---\n\n";
                }

            }

        }
    }
    int writeContent(byte[] bytes, int i)
    {
        int numProperty, byteProperty, offset, nameLen;
        string name;
        name = "";
        offset = decimalVersion4(bytes, i);
        checkHierarchy(offset);

        #region debug
        //For Debug
        if (i > 130000)
        {
            i = i;
            return fileLen;
        }
        #endregion
        if (offset == 0)
        {
            i += 13;
            i--;
            return i;
        }
        else if ((offset > fileLen) && (i > 2300))
        {

            i += 16;
            while (bytes[i] == 0)
            {
                i++;
            }
            if (fbxVersion == decimalVersion4(bytes, i))
            {
                debug1 += _tab(depth) + "--------End--------";
                i = fileLen;
                return i;
            }
        }

        i += 4;

        numProperty = decimalVersion4(bytes, i);
        i += 4;

        byteProperty = decimalVersion4(bytes, i);
        i += 4;

        nameLen = decimalVersion(bytes[i]);


        for (int j = 1; j <= nameLen; j++)
        {
            if (i + j < fileLen)
            {
                name += Byte2Str(bytes[i + j]);
            }
        }

        i += nameLen;
        if (numProperty != 0)
        {
            /*
            debug1 += _tab(depth) + "Name:" + name + "\n";
            debug1 += _tab(depth) + "NameLength: " + nameLen + "\n";
            debug1 += _tab(depth) + "Offset: " + offset + "\n";
            debug1 += _tab(depth) + "PropetyNum: " + numProperty + "\n";
            debug1 += _tab(depth) + "PropetyBytes: " + byteProperty + "\n";
            */
            debug1 += _tab(depth) + "\t" + name + ":\n" + _tab(depth) + "\t{" + "\n";
            for (int _prop = 0; _prop < numProperty; _prop++)
            {
                i++;

                print(i);
                if (i < fileLen)
                {
                    string typeName = Byte2Str(bytes[i]);
                    //debug1 += _tab(depth) + "\tType[" + (_prop + 1) + "]: " + typeName + "\n";
                    switch (typeName)
                    {
                        case "C":
                            bool ToF = (bytes[i + 1] % 2 == 1) ? true : false;
                            //debug1 += _tab(depth) + "\tBool[" + (_prop + 1) + "]: " + ToF + "\n\n";
                            debug1 += _tab(depth) + "\t\tBool: " + ToF + "\n";
                            i++;
                            break;
                        case "I":
                            debug1 += _tab(depth) + "\t\tI: " + decimalVersion4(bytes, i + 1) + "\n";
                            i += 4;
                            break;
                        case "D":
                            int[] doubleBox, double8byte;
                            doubleBox = new int[8];
                            double8byte = new int[64];
                            debug1 += _tab(depth) + "\t\tD: ";
                            for (int _i = 1; _i <= 8; _i++)
                            {
                                //debug1 += bytes[i + _i] + " ";
                            }
                            //debug1 += "\n";
                            //debug1 += _tab(depth);
                            for (int digit8 = 7; digit8 >= 0; digit8--)
                            {
                                doubleBox = byte2Binairy(bytes[i + digit8 + 1]);

                                for (int digit1 = 7; digit1 >= 0; digit1--)
                                {
                                    double8byte[digit8 * 8 + digit1] = doubleBox[digit1];
                                    //Binary is being commented out !
                                    // debug1 += box8byte[digit8 * 8 + digit1];
                                }
                                //debug1 += " ";
                            }
                            //debug1 += "\n";
                            //debug1 += _tab(depth);
                            debug1 += binary2double(double8byte).ToString("F3");
                            debug1 += "\n";
                            i += 8;
                            break;
                        case "L":
                            int[] longBox, long8byte;
                            longBox = new int[8];
                            long8byte = new int[64];
                            debug1 += _tab(depth) + "\t\tL: ";
                            for (int digit8 = 7; digit8 >= 0; digit8--)
                            {
                                longBox = byte2Binairy(bytes[i + digit8 + 1]);

                                for (int digit1 = 7; digit1 >= 0; digit1--)
                                {
                                    long8byte[digit8 * 8 + digit1] = longBox[digit1];

                                }
                            }
                            debug1 += hexadecimal(binary2long(long8byte));
                            debug1 += "\n";
                            i += 8;
                            break;
                        case "R":
                            int dataLength = decimalVersion4(bytes, i + 1);
                            debug1 += _tab(depth) + "\tDataLength[" + (_prop + 1) + "]: " + dataLength + "\n";
                            i += 4;
                            debug1 += _tab(depth) + "\t\tR: ";
                            for (int k = 1; k <= dataLength; k++)
                            {
                                if (i + k < fileLen)
                                {
                                    debug1 += bytes[i + k] + "|";
                                }
                                else
                                {
                                    print("out of range");
                                }
                            }
                            debug1 += "\n";
                            i += dataLength;
                            break;

                        case "S":

                            int stringLength = decimalVersion4(bytes, i + 1);
                            // debug1 += _tab(depth) + "\tDataLength[" + (_prop + 1) + "]: " + stringLength + "\n";
                            i += 4;
                            if (stringLength != 0)
                            {
                                debug1 += _tab(depth) + "\t\tS: ";
                                debug1 += "\"";
                                for (int k = 1; k <= stringLength; k++)
                                {

                                    if (i + k < fileLen)
                                    {
                                        debug1 += Byte2Str(bytes[i + k]);
                                    }
                                    else
                                    {
                                        print("out of range");
                                    }
                                }
                                debug1 += "\"";

                            }
                            else
                            {
                                debug1 += _tab(depth) + "\t\tS: \"\" ";

                            }
                            debug1 += "\n";
                            i += stringLength;

                            break;

                        case "d":
                            int dArrayLength = decimalVersion4(bytes, i + 1);
                            debug1 += _tab(depth) + "\tDataLength[" + (_prop + 1) + "]: " + dArrayLength + "\n";
                            i += 4;

                            int Encoding = decimalVersion4(bytes, i + 1);
                            debug1 += _tab(depth) + "\tEncoding[" + (_prop + 1) + "]: " + Encoding + "\n";
                            i += 4;

                            int CompressedLength = decimalVersion4(bytes, i + 1);
                            debug1 += _tab(depth) + "\tCompressedLength[" + (_prop + 1) + "]: " + CompressedLength + "\n";
                            i += 4;


                            byte[] zlib = new byte[CompressedLength];
                            int[] zlibBinaire = new int[CompressedLength * 8];
                            binaireList8 = new List<int>();

                            for (int k = 0; k < CompressedLength; k++)
                            {
                                if (Encoding == 1)
                                {
                                    zlib[k] = bytes[i + k + 1];
                                    binaireList8 = addBinaryList(binaireList8, zlib[k]);
                                    debug1 += bytes[i + k + 1] + " ";
                                    if (k % 8 == 7)
                                    {
                                        debug1 += "\n";
                                    }
                                }
                            }
                            readEncodedList(binaireList8);

                            for (int i_ = 0; i_ < Doubles.Count; i_++)
                            {
                                debug1 += Doubles[i_] + " ";
                                if (i_ % 32 == 31)
                                {
                                    debug1 += "\n";
                                }
                            }
                            debug1 += "\n";
                            writeText(debug1, true);
                            int ListLength = Doubles.Count;
                            int verticeIndex = 0;
                            for (int k = ListLength - 1; k > 0; k -= 8)
                            {
                                print("k: "+k);
                                if(k<0)
                                {
                                    k = k;
                                }
                                int[] doubleArrayBox, doubleArray4byte;
                                doubleArrayBox = new int[8];
                                doubleArray4byte = new int[64];

                                for (int digit8 = 0; digit8 < 8; digit8++)
                                {
                                    doubleArrayBox = byte2Binairy((byte)Doubles[-digit8 + k]);
                                    print("-digit8 + k: " + -digit8 + k);
                                    for (int digit1 = 7; digit1 >= 0; digit1--)
                                    {
                                        print("(7 - digit8) * 8 + digit1: " + (7 - digit8) * 8 + digit1);
                                        doubleArray4byte[(7 - digit8) * 8 + digit1] = doubleArrayBox[digit1];

                                    }

                                }


                                if (k % 3 == 2)
                                {
                                    debug1 += _tab(depth + 2) + "[" + verticeIndex + "]( ";
                                    verticeIndex++;
                                }
                                debug1 += binary2double(doubleArray4byte).ToString("f1");
                                if (k % 24 != 7)
                                {
                                    debug1 += ", ";
                                }
                                else
                                {
                                    debug1 += ")\n";
                                }



                            }
                            debug1 += "\n";
                            i += CompressedLength;


                            break;

                        case "i":
                            int iArrayLength = decimalVersion4(bytes, i + 1);
                            debug1 += _tab(depth) + "\tDataLength[" + (_prop + 1) + "]: " + iArrayLength + "\n";
                            i += 4;

                            int iEncoding = decimalVersion4(bytes, i + 1);
                            debug1 += _tab(depth) + "\tEncoding[" + (_prop + 1) + "]: " + iEncoding + "\n";
                            i += 4;

                            int iCompressedLength = decimalVersion4(bytes, i + 1);
                            debug1 += _tab(depth) + "\tCompressedLength[" + (_prop + 1) + "]: " + iCompressedLength + "\n";
                            i += 4;

                            debug1 += _tab(depth) + "\ti:\n\t" + _tab(depth + 2);

                            byte[] zlib_i = new byte[iCompressedLength];
                            int[] zlibBinaire_i = new int[iCompressedLength * 8];
                            binaireList8 = new List<int>();

                            for (int k = 0; k < iCompressedLength; k++)
                            {
                                debug1 += bytes[i + k + 1] + " ";
                                if (iEncoding == 1)
                                {
                                    zlib_i[k] = bytes[i + k + 1];
                                    binaireList8 = addBinaryList(binaireList8, zlib_i[k]);
                                }
                                if (k % 8 == 7)
                                {
                                    debug1 += "\n";
                                }
                            }
                            readEncodedList(binaireList8);


                            for (int k = 0; k < iCompressedLength; k += 4)
                            {
                                int kazu = decimalVersion4(bytes, i + k + 1);
                                if (kazu < 0)
                                {
                                    kazu *= -1;
                                    kazu -= 1;
                                }
                                if (iCompressedLength - 4 > k)
                                {
                                    debug1 += kazu + ", ";
                                }
                                else
                                {
                                    debug1 += kazu;
                                }
                                //                                debug1 += bytes[i + k + 1].ToString("x") + " ";
                                if (k % 16 == 12)
                                {
                                    debug1 += "\n\t" + _tab(depth + 2);

                                }

                            }
                            /*
                            for (int k = 0; k < iCompressedLength; k += 4)
                            {
                                if (i + k < fileLen)
                                {
                                    int[] intBox, int4byte;
                                    intBox = new int[8];
                                    int4byte = new int[32];

                                    for (int digit8 = 3; digit8 >= 0; digit8--)
                                    {
                                        intBox = byte2Binairy(bytes[i + digit8 + k + 1]);

                                        for (int digit1 = 7; digit1 >= 0; digit1--)
                                        {
                                            int4byte[digit8 * 8 + digit1] = intBox[digit1];
                                        }

                                    }

                                    debug1 += hexadecimal(binary2int(int4byte)) + "|";
                                }
                                else
                                {
                                    print("out of range");
                                }
                            }
                            */
                            debug1 += "\n";
                            i += iCompressedLength;


                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    print("Maybe Index Out Of Range: " + i);
                }

            }
            debug1 += _tab(depth) + "\t}" + "\n\n";
        }
        else
        {
            debug1 += _tab(depth) + "---Start of Nested list---\n\n";
            ParentList.Add(offset);
            depth++;
            debug1 += _tab(depth) + name + ":\n" + _tab(depth) + "{" + "\n";
            /*
            debug1 += _tab(depth - 1) + "Name:" + name + "\n";
            debug1 += _tab(depth - 1) + "NameLength: " + nameLen + "\n";
            debug1 += _tab(depth - 1) + "Offset: " + offset + "\n";
            debug1 += _tab(depth - 1) + "PropetyNum: " + numProperty + "\n";
            debug1 += _tab(depth - 1) + "PropetyBytes: " + byteProperty + "\n\n";
            */
        }


        return i;
    }

    /// <summary>
    /// Deflate compress
    /// </summary>
    /// <param name="list"></param>
    void readEncodedList(List<int> list)
    {
        //CM
        int CMeth, CInfo;
        int[] CM_4bit = new int[4];
        int[] CINFO_4bit = new int[4];
        for (int i = 0; i < 4; i++)
        {
            CM_4bit[i] = list[i];
            CINFO_4bit[i] = list[i + 4];
        }
        CMeth = binary2int(CM_4bit, 4);
        CInfo = binary2int(CINFO_4bit, 4);
        //FLG
        int FCheck, FLevel;
        int[] FCHECK_5bit = new int[5];
        int FDICT;
        int[] FLEVEL_2bit = new int[2];
        for (int i = 0; i < 5; i++)
        {
            FCHECK_5bit[i] = list[i + 8];
        }
        FDICT = list[13];
        FLEVEL_2bit[0] = list[14];
        FLEVEL_2bit[1] = list[15];

        FCheck = binary2int(FCHECK_5bit, 5);
        FLevel = binary2int(FLEVEL_2bit, 2);

        int[] CM_8bit = new int[8], FLG_8bit = new int[8];

        for (int i = 0; i < 8; i++)
        {
            CM_8bit[i] = list[i];
            FLG_8bit[i] = list[i + 8];
        }
        int CM, FLG;
        CM = binary2int(CM_8bit, 8);
        FLG = binary2int(FLG_8bit, 8);
        /*
                debug1 += _tab(depth) + "Compression Method: " + CMeth + " ";
                debug1 += "Compression Info: " + CInfo + "\n";
                debug1 += _tab(depth) + "Flug Check: " + FCheck + " ";
                debug1 += "Preset Dictionary: " + FDICT + " ";
                debug1 += "Flug Level: ";
                switch (FLevel)
                {
                    case 0:
                        debug1 += "Compressor used fastest algorithm\n";
                        break;
                    case 1:
                        debug1 += "Compressor used fast algorithm\n";
                        break;
                    case 2:
                        debug1 += "Compressor used default algorithm\n";
                        break;
                    case 3:
                        debug1 += "Compressor used maximum compression, slowest algorithm\n";
                        break;
                    default:
                        debug1 += "Something wrong with Flug Level\n";
                        break;
                }
                */




        if ((CM * 256 + FLG) % 31 == 0)
        {
            Codes = new List<int>();
            Doubles = new List<int>();
            BlockHeader(list, 16);
        }
        else
        {
            debug1 += _tab(depth) + "Something wrong with the header file\n";
        }


    }
    /// <summary>
    /// For each block of array
    /// </summary>
    /// <param name="list"></param>
    /// <param name="i_startPos"></param>
    void BlockHeader(List<int> list, int i_startPos)
    {
        //    debug1 += _tab(depth) + "FLUG CHECK OK\n";

        int BFINAL, BTYPE;
        int[] BTYPE_2bit = new int[2];
        BFINAL = list[i_startPos];

        BTYPE_2bit[0] = list[i_startPos + 1];
        BTYPE_2bit[1] = list[i_startPos + 2];
        /*
         j(list)  || 18| 17|
         i(BTYPE) || 1 | 0 |    17 | 18
         BTYPE[i] || 1 | 0 | ->  0 | 1

         */
        BTYPE = binary2int(BTYPE_2bit, 2);
        bool IsFinal = (BFINAL == 0) ? false : true;
        switch (BTYPE)
        {
            case 0:
                break;
            case 1:
                FixedHuffman(list, i_startPos + 3, IsFinal);
                Doubles = ExtensionFile(Codes);
                break;
            case 2:
                CustomHuffman(list, i_startPos + 3, IsFinal);
                if (Doubles.Count == 0)
                { Doubles.AddRange(ExtensionFile(Codes)); }
                break;
            case 3:
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// Once Codes is done, we can finally the uncompressed data
    /// </summary>
    /// <param name="codelist"></param>
    /// <returns></returns>
    List<int> ExtensionFile(List<int> codelist)
    {
        int index = 0;
        List<int> _list = new List<int>();
        for (int i = 0; i < codelist.Count; i++)
        {
            print("index: " + index);
            if (codelist[i] >= 0)
            {
                _list.Add(codelist[i]);
                index++;
            }
            else if (codelist[i] == -1)
            {
                int length = codelist[i + 1];
                int distance = codelist[i + 2];

                for (int j = 0; j < length; j++)
                {
                    if (j - distance >= 0)
                    {
                        int k = j - distance - 1;
                        _list.Add(_list[index - distance + k]);
                    }
                    else
                    {
                        _list.Add(_list[index - distance + j]);
                    }
                }

                index += length;
                i += 3;

            }
        }
        //debug1 += "List: ";
        for (int count = 0; count < _list.Count; count++)
        {
            //  debug1 += _list[count] + " ";
            //  if(count%8==7)
            // debug1+="| ";
        }
        // debug1 += " [Size]: "+_list.Count+"\n";

        return _list;
    }

    #region Custom Huffman
    void CustomHuffman(List<int> list, int startPos, bool IsFinal)
    {
        int currentPos = startPos;
        int HLIT, HDIST, HCLEN;
        int[] HLIT5bit, HDIST5bit, HCLEN4bit;

        //Length of Value/Length code
        //n: 0-29 to add 257 on 
        HLIT5bit = new int[5];
        HLIT5bit = chargeList(list, HLIT5bit, currentPos, 5);
        currentPos += 5;
        HLIT = binary2int(HLIT5bit) + 257;
        print("<color=blue>" + HLIT + "</color>");

        //Length of distance code
        //n: 0-31 to add 1 on 
        HDIST5bit = new int[5];
        HDIST5bit = chargeList(list, HDIST5bit, currentPos, 5);
        currentPos += 5;
        HDIST = binary2int(HDIST5bit) + 1;
        print("<color=blue>" + HDIST + "</color>");

        //Length of Length code list
        //n: 0-15 to add 4 on 
        HCLEN4bit = new int[4];
        HCLEN4bit = chargeList(list, HCLEN4bit, currentPos, 4);
        currentPos += 4;
        HCLEN = binary2int(HCLEN4bit) + 4;
        print("<color=blue>" + HCLEN + "</color>");

        // Length of Length Code(3bit par length)
        //16 17 18 0 8 7 9 6 10 5 11 4 12 3 13 2 14 1 15
        int[] LLOLL_bit = new int[3 * HCLEN];
        int[] LengthList = new int[19];

        for (int i = 0; i < HCLEN; i++)
        {
            LLOLL_bit[3 * i + 0] = list[currentPos + 3 * i + 0];
            LLOLL_bit[3 * i + 1] = list[currentPos + 3 * i + 1];
            LLOLL_bit[3 * i + 2] = list[currentPos + 3 * i + 2];
            LengthList[i] = binary2int(LLOLL_bit, 3 * i, 3);
        }
        currentPos += HCLEN * 3;
        // Fill the tail of the Length Code with 0 if it is omitted(normally 2, 14, 1, 15)
        if (HCLEN < 19)
        {
            int filler = HCLEN;
            while (filler < 19)
            {
                LengthList[filler] = 0;
                filler++;
            }

        }

        //Put the array in order
        // 0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18
        LengthList = IndexwiseOrderedIntArray(LengthList);

        //Memorize the set of index and length in ascending order of length (if length is same, see index)
        Dictionary<int, int> DictLLoLL = new Dictionary<int, int>();
        List<int> IndexLLoLL = new List<int>();
        List<string> PrefixList = new List<string>();
        IndexLLoLL = ValuewiseLLoLL(LengthList);
        DictLLoLL = GenerateLLOLLDict(LengthList);

        PrefixList = CreatePrefixCode(DictLLoLL, IndexLLoLL);
        //Start to read a Value/Length and distance code
        List<int> LList = new List<int>();
        List<int> DList = new List<int>();
        LList = GetList(HLIT, list, PrefixList, currentPos, IndexLLoLL);
        currentPos = curPos;
        DList = GetList(HDIST, list, PrefixList, currentPos, IndexLLoLL);
        currentPos = curPos;

        //Finally Start To Read a compressed list


        Dictionary<int, List<int>> LengthDict = new Dictionary<int, List<int>>();
        Dictionary<int, List<string>> StrLengthDict = new Dictionary<int, List<string>>();
        Dictionary<int, List<int>> DistDict = new Dictionary<int, List<int>>();
        Dictionary<int, List<string>> StrDistDict = new Dictionary<int, List<string>>();
        LengthDict = ElementsInList(LList, LengthDict, LengthList);
        StrLengthDict = BinaryDict(LengthDict);
        DistDict = ElementsInList(DList, DistDict, LengthList);
        StrDistDict = BinaryDict(DistDict);

        //TODO 
        List<string> LengthAccordList = new List<string>();
        List<string> DistanceList = new List<string>();

        LengthAccordList = CompleteList(StrLengthDict, LengthDict, HLIT);
        DistanceList = CompleteList(StrDistDict, DistDict, HDIST);

        int[] LData = SearchIndexStepAndData(list, currentPos, 1, LengthAccordList);
        int[] DData;
        int[] TrueLengthData, TrueDistanceData;


        string numAccordLength = BinaryString;
        //For the first time it can't be more than 256
        int step = LData[1];
        currentPos += step;
        int value = LengthAccordList.IndexOf(numAccordLength);
        Codes.Add(value);
        while (value != 256)
        {
            //currentPos = 699 de tomaru
            LData = SearchIndexStepAndData(list, currentPos, 1, LengthAccordList);
            numAccordLength = BinaryString;
            step = LData[1];
            currentPos += step;

            value = LengthAccordList.IndexOf(numAccordLength);

            if (value > 256)
            {
                //TODO

                TrueLengthData = LengthOfValue(value, list, currentPos);
                int length = TrueLengthData[0];
                currentPos += TrueLengthData[1];

                DData = SearchIndexStepAndData(list, currentPos, 1, DistanceList);
                numAccordLength = BinaryString;
                step = DData[1];
                currentPos += step;
                int dist = DistanceList.IndexOf(numAccordLength);

                TrueDistanceData = DistanceOfValue(dist, list, currentPos);
                int distance = TrueDistanceData[0];
                currentPos += TrueDistanceData[1];

                Codes.Add(-1);
                Codes.Add(length);
                Codes.Add(distance);
                Codes.Add(-2);
            }
            else
            {
                Codes.Add(value);
            }

        }
        if (!IsFinal)
        {
            BlockHeader(list, currentPos);
    
        }
        debug1 += "\n";
    }
    /// <summary>
    /// return distance and extrabit needed
    /// </summary>
    /// <param name="distCode"></param>
    /// <param name="list"></param>
    /// <param name="startPos"></param>
    /// <returns>0:distance, 1:extrabit</returns>
    int[] DistanceOfValue(int distCode, List<int> list, int startPos)
    {

        int extraDistBit;
        int[] result = new int[2];

        if (0 <= distCode && distCode < 4)
        {
            extraDistBit = 0;
            result[0] = distCode + 1;
            result[1] = extraDistBit;

        }
        else
        {
            extraDistBit = ((distCode - 3) / 2);
            if (distCode % 2 == 0)
            {
                extraDistBit++;
            }
        }


        int distance = (int)Mathf.Pow(2, extraDistBit) * (distCode % 2 + 2) + 1;


        for (int i = 0; i < extraDistBit; i++)
        {
            distance += list[startPos + i] * (int)Mathf.Pow(2, i);
        }

        result[0] = distance;
        result[1] = extraDistBit;

        return result;
    }
    /// <summary>
    /// Convert the code (257-> 3) for ex.
    /// </summary>
    /// <param name="wordCode"></param>
    /// <param name="list"></param>
    /// <param name="startPos"></param>
    /// <returns></returns>
    int[] LengthOfValue(int wordCode, List<int> list, int startPos)
    {
        int[] result = new int[2];
        int lengthAccord = -1, extraLengthBit = -1;
        if (257 <= wordCode && wordCode < 265)
        {
            lengthAccord = wordCode - 254;
            extraLengthBit = 0;
        }
        else if (265 <= wordCode && wordCode < 269)
        {
            int difference = wordCode - 265;
            lengthAccord = 11 + difference * 2;
            extraLengthBit = 1;
            lengthAccord += list[startPos];
        }
        else if (269 <= wordCode && wordCode < 273)
        {
            int difference = wordCode - 269;
            lengthAccord = 19 + difference * 4;
            extraLengthBit = 2;
            lengthAccord += list[startPos] + list[startPos + 1] * 2;
        }
        else if (273 <= wordCode && wordCode < 277)
        {
            int difference = wordCode - 273;
            lengthAccord = 35 + difference * 8;
            extraLengthBit = 3;
            lengthAccord += list[startPos] + list[startPos + 1] * 2 + list[startPos + 2] * 4;
        }
        else if (277 <= wordCode && wordCode < 281)
        {
            int difference = wordCode - 277;
            lengthAccord = 67 + difference * 8;
            extraLengthBit = 4;
            lengthAccord += list[startPos] + list[startPos + 1] * 2 + list[startPos + 2] * 4 + list[startPos + 3] * 8;
        }
        else if (281 <= wordCode && wordCode < 285)
        {
            int difference = wordCode - 281;
            lengthAccord = 131 + difference * 16;
            extraLengthBit = 5;
            lengthAccord += list[startPos] + list[startPos + 1] * 2 + list[startPos + 2] * 4 + list[startPos + 3] * 8 + list[startPos + 4] * 16;
        }
        else if (wordCode == 285)
        {
            extraLengthBit = 0;
            lengthAccord = 258;
        }
        result[0] = lengthAccord;
        result[1] = extraLengthBit;

        return result;
    }

    List<string> CompleteList(Dictionary<int, List<string>> valueDict, Dictionary<int, List<int>> indexDict, int n)
    {
        List<string> result = new List<string>();
        for (int i = 0; i < n; i++)
        {
            bool IsFound = false;
            for (int len = 0; len < 19; len++)
            {
                if (indexDict.ContainsKey(len) && !IsFound)
                {

                    for (int j = 0; j < indexDict[len].Count; j++)
                    {
                        if (indexDict[len][j] == i)
                        {
                            string val = valueDict[len][j];
                            #region debug

                            if (j==3)
                            {
                                j = j;
                            }
                            #endregion
                            result.Add(val);
                            #region debug

                            print("i: " + i);
                            print("length: " + len);
                            print("j: " + j);
                            print("value: " +val);
                            IsFound = true;
                            break;

                            #endregion
                        }
                    }

                }
            }
            if (!IsFound)
            {
                result.Add("");
            }
        }
        return result;
    }
    //(Length, List of Value)
    Dictionary<int, List<string>> BinaryDict(Dictionary<int, List<int>> lengthDict)
    {
        bool IsStarted = false;
        bool OldChanged = false;
        string value = "";
        Dictionary<int, List<string>> dict = new Dictionary<int, List<string>>();
        for (int i = 0; i < 16; i++)
        {
            if (lengthDict.ContainsKey(i))
                {
                    List<string> v_list = new List<string>();
                    bool IsChanged = false;
                for (int j = 0; j < lengthDict[i].Count; j++)
                {
                    if (i == 0)
                    {
                        v_list.Add("");
                    }
                    else
                    {
                        if (!IsStarted)
                        {
                            IsStarted = true;
                            IsChanged = true;
                            OldChanged = true;
                            value = value.ToString().PadRight(i, '0');
                            v_list.Add(value);
                        }
                        else
                        {
                            if (!IsChanged)
                            {
                                IsChanged = true;
                                if (OldChanged == false)
                                {
                                    value = PlusDigitBase2(value);
                                }
                                else
                                {
                                    value = PlusOneBase2(value);
                                    value = PlusDigitBase2(value);
                                }

                                value = value.PadLeft(i, '0');
                            }
                            else
                            {
                                value = PlusOneBase2(value);
                                value = value.PadLeft(i, '0');
                            }
                            OldChanged = true;
                            v_list.Add(value);
                        }
                    }

                }
                if (!IsChanged)
                {
                    if (OldChanged == true)
                    {
                        value = PlusOneBase2(value);
                    }
                    value = PlusDigitBase2(value);
                    OldChanged = false;
                }
                
                
                dict.Add(i, v_list);

                
            }
        }
        return dict;
    }

    //(Length,List of Index)
    Dictionary<int, List<int>> ElementsInList(List<int> LengthOrDistanceList, Dictionary<int, List<int>> dict, int[] LengthList)
    {
        for (int i = 0; i < 19; i++)
        {
            if (LengthList[i] != 0)
            {
                List<int> ListForDictionary = new List<int>();
                for (int j = 0; j < LengthOrDistanceList.Count; j++)
                {
                    if (i == LengthOrDistanceList[j])
                    {
                        ListForDictionary.Add(j);
                    }

                }
                dict.Add(i, ListForDictionary);
            }
        }
        return dict;
    }

   

    List<int> GetList(int Limit, List<int> list, List<string> PrefixList, int startPos, List<int> IndexList)
    {
        int currentPos = startPos;


        //0 - 285 or 0 - 31
        List<int> RealLLoLL = new List<int>();
        int k = 0;
        int _posStart = 0, _posEnd;
        int oldInt = -1;
        while (k < Limit)
        {
            #region debug
            if (k > 260)
            {
                k = k;
            }
            #endregion
            int[] IndexAndNum;
            int value;
            IndexAndNum = SearchIndexStepAndData(list, currentPos, 1, PrefixList);

            currentPos += (int)IndexAndNum[1];
            value = IndexList[(int)IndexAndNum[0]];
            if (value < 16)
            {
                //Only if value < 16(Not to add 16,17,18 as value)
                oldInt = value;
                RealLLoLL.Add(value);
            }
            print("<color=red> k: " + k + "</color>");
            print("<color=red> value: " + value + "</color>");


            print("<color=green> Num: " + BinaryString + "</color>");
            int[] extraBit;
            int ex = 1;

            switch (value)
            {
                case 16:

                    extraBit = new int[2];
                    extraBit = chargeList(list, extraBit, currentPos, 2);
                    print(extraBit[1] + " " + extraBit[0]);
                    currentPos += 2;
                    ex = binary2int(extraBit) + 3;
                    print(ex);
                    _posEnd = _posStart + ex;
                    for (int j = _posStart; j < _posEnd; j++)
                    {
                        if (oldInt != -1 && oldInt >= 0)
                        {
                            RealLLoLL.Add(oldInt);
                        }
                        else
                        {
                            print("Something wrong in switch(value)");
                        }
                        _posStart = _posEnd;
                    }
                    break;
                case 17:

                    extraBit = new int[3];
                    extraBit = chargeList(list, extraBit, currentPos, 3);
                    print(extraBit[2] + " " + extraBit[1] + " " + extraBit[0]);
                    currentPos += 3;
                    ex = binary2int(extraBit) + 3;
                    _posEnd = _posStart + ex;
                    for (int j = _posStart; j < _posEnd; j++)
                    {
                        RealLLoLL.Add(0);

                    }
                    _posStart = _posEnd;
                    oldInt = 0;
                    break;
                case 18:

                    extraBit = new int[7];
                    extraBit = chargeList(list, extraBit, currentPos, 7);
                    currentPos += 7;
                    ex = binary2int(extraBit) + 11;
                    _posEnd = _posStart + ex;
                    for (int j = _posStart; j < _posEnd; j++)
                    {
                        RealLLoLL.Add(0);
                    }
                    _posStart = _posEnd;
                    oldInt = 0;
                    break;
                default:
                    break;
            }

            k += ex;

        }
        curPos = currentPos;
        return RealLLoLL;
    }
    /// <summary>
    /// Find binary data and return it and its length
    /// </summary>
    /// <param name="list"></param>
    /// <param name="startPos"></param>
    /// <param name="Step"></param>
    /// <param name="prefixList"></param>
    /// <returns></returns>
    int[] SearchIndexStepAndData(List<int> list, int startPos, int Step, List<string> prefixList)
    {
        int[] result = new int[2];

        string teStr = "";
        for (int i = 0; i < Step; i++)
        {
            teStr += list[startPos + i];
        }
        if (Step > 19)
        {
            result[0] = -1;
            result[1] = Step;
            BinaryString = teStr;
            print("Error in SearchIndexStepAndData");
            return result;
        }

        int index = IndexOfPrefix(prefixList, teStr);
        if (index >= 0)
        {
            result[0] = index;
            result[1] = Step;
            BinaryString = teStr;
            return result;
        }
        else
        {
            return SearchIndexStepAndData(list, startPos, ++Step, prefixList);
        }

    }
    int IndexOfPrefix(List<string> LiStr, string str)
    {
        for (int i = 0; i < LiStr.Count; i++)
        {
            if (Equals(LiStr[i], str))
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Order the int array from lowest value to highest value
    /// </summary>
    /// <param name="InputLengthList"></param>
    /// <returns></returns>
    List<int> ValuewiseLLoLL(int[] InputLengthList)
    {
        List<int> result = new List<int>();
        for (int searchedValue = 1; searchedValue < 19; searchedValue++)
        {
            for (int j = 0; j < 19; j++)
            {
                if (InputLengthList[j] == searchedValue)
                {
                    result.Add(j);
                }
            }
        }
        return result;
    }
    Dictionary<int, int> GenerateLLOLLDict(int[] InputLengthList)
    {

        Dictionary<int, int> IndexValueOfLLOLL = new Dictionary<int, int>();

        for (int searchedValue = 1; searchedValue < 19; searchedValue++)
        {
            for (int j = 0; j < 19; j++)
            {
                if (InputLengthList[j] == searchedValue)
                {
                    IndexValueOfLLOLL.Add(j, searchedValue);
                }
            }
        }
        return IndexValueOfLLOLL;
    }

    /// <summary>
    /// Create a list of prefix codes
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    List<string> CreatePrefixCode(Dictionary<int, int> dict, List<int> list)
    {
        string current = "0";
        int currentLen = 1;
        List<string> base2list = new List<string>();
        //Index 0
        if (dict[list[0]] == 1)
        {

            base2list.Add(current);
            current = "1";
        }
        else if (dict[list[0]] < 19)
        {
            for (int ji = 1; ji < dict[list[0]]; ji++)
            {
                current += "0";
                currentLen++;
           
            }
            base2list.Add(current);
            current = PlusOneBase2(current);

            current = current.PadLeft(dict[list[0]], '0');
        }
        else
        {
            print("something wrong in CreatePrefixCode");
        }
        //Index 1,2,3,...
        for (int i = 1; i < list.Count; i++)
        {
            int LengthV = dict[list[i]];

            if (currentLen == LengthV)
            {
                // Leng.
                //  1 -> 1   2

                if (checkAllDigitsOne(current))
                {
                    //ex.11111
                    base2list.Add(current);
                    print("Full 1");
                }
                else
                {
                    //ex.11011 ->11100
                    base2list.Add(current);
                    current = PlusOneBase2(current);
                }
            }
            else
            {
                // Leng.
                //  1    1 -> 2
                while (currentLen != LengthV && currentLen < 19)
                {
                    current = PlusDigitBase2(current);
                    currentLen++;
                }
                base2list.Add(current);
                current = PlusOneBase2(current);
            }
        }

        return base2list;
    }
    /// <summary>
    /// Return true if every digit is '1'
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    bool checkAllDigitsOne(string str)
    {
        foreach (char c in str)
        {
            if (c == '0')
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Addition of 1 in binary(ex. 11000 -> 11001)
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    string PlusOneBase2(string num)
    {

        //binary to int(ex.11000 -> 24)
        int value = Convert.ToInt32(num, 2);
        //Plus one(25)
        value++;
        //int to binary(25 -> 11001)
        return Convert.ToString(value, 2);

    }
    /*
        int StrBinary2Int(string value)
        {
            foreach(char c in value)
            {
                if(c != '0' && c!= '1')
                {
                    return -1;
                }
            }
            int result = 0;
            for(int i = 0;i<value.Length;i++)
            {
                result += i*Convert.ToInt32( value[i]
            }
        }
        */
        /// <summary>
        /// Add '0' at the end of the digit
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
    string PlusDigitBase2(string num)
    {
        return num + "0";
    }
    /// <summary>
    //                              ... <- 11 <- 12 <- 13 <- 14 <- 15
    //Remind                     ... <-  5 <-  4 <-  3 <-  2 <-  1
    //Previ:  0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18
    //Value:  0  0  7  7  5  5  5  7  7  5  1  2  7  7  7  0  4  7  0

    //Follo: 16 17 18  0  8  7  9  6 10  5 11  4 12  3 13  2 14  1 15

    /// </summary>
    /// <param name="InputIntArray"></param>
    /// <returns></returns>
    int[] IndexwiseOrderedIntArray(int[] InputIntArray)
    {
    
        if (InputIntArray.Length == 19)
        {
            int[] result = new int[19];
            result[0] = InputIntArray[3];
            result[1] = InputIntArray[17];
            result[2] = InputIntArray[15];
            result[3] = InputIntArray[13];
            result[4] = InputIntArray[11];
            result[5] = InputIntArray[9];
            result[6] = InputIntArray[7];
            result[7] = InputIntArray[5];
            result[8] = InputIntArray[4];
            result[9] = InputIntArray[6];
            result[10] = InputIntArray[8];
            result[11] = InputIntArray[10];
            result[12] = InputIntArray[12];
            result[13] = InputIntArray[14];
            result[14] = InputIntArray[16];
            result[15] = InputIntArray[18];
            result[16] = InputIntArray[0];
            result[17] = InputIntArray[1];
            result[18] = InputIntArray[2];
            return result;
        }
        else
        {
            print("<color = red>Input array has no good length.</color>");
            return null;
        }
    }
    int[] chargeListInv(List<int> originalList, int[] ToThisArray, int startPos, int elemNum)
    {
        for (int i = 0; i < elemNum; i++)
        {
            ToThisArray[elemNum - i - 1] = originalList[startPos + i];
        }
        return ToThisArray;
    }
    int[] chargeList(List<int> originalList, int[] ToThisArray, int startPos, int elemNum)
    {
        for (int i = 0; i < elemNum; i++)
        {
            ToThisArray[i] = originalList[startPos + i];
        }
        return ToThisArray;
    }
    #endregion

    #region Fixed Huffman
    void FixedHuffman(List<int> list, int startPos, bool IsFinal)
    {
        int lengthAccord, extraLengthBit;
        int distance, extraDistBit;
        int currentIndex = startPos;
        int wordCode, diff, distCode;
        int[] Lvalue, Dvalue; int[] min_8bit;
        int numPrefix, min;
        int[] prefix;
        debug1 += _tab(depth) + "\t\td: \n";

        while (currentIndex < list.Count)
        {

            if (currentIndex > 700)
                currentIndex = currentIndex;

            prefix = new int[5];
            for (int i = 0; i < 5; i++)
            {
                prefix[i] = list[currentIndex + i];
            }
            //READ
            if (prefix[0] == 0)
            {
                if (prefix[1] == 0)
                {
                    if (prefix[2] == 0)
                    {
                        numPrefix = 7;
                        min = 256;
                    }
                    else
                    {
                        if (prefix[3] == 0)
                        {
                            numPrefix = 7;
                            min = 256;
                        }
                        else
                        {
                            numPrefix = 8;
                            min = 0;
                        }
                    }
                }
                else
                {
                    numPrefix = 8;
                    min = 0;
                }
            }
            else
            {
                if (prefix[1] == 0)
                {
                    numPrefix = 8;
                    min = 0;
                }
                else
                {
                    if (prefix[2] == 0)
                    {
                        if (prefix[3] == 0)
                        {
                            if (prefix[4] == 0)
                            {
                                numPrefix = 8;
                                min = 280;
                            }
                            else
                            {
                                numPrefix = 9;
                                min = 144;
                            }
                        }
                        else
                        {
                            numPrefix = 9;
                            min = 144;
                        }
                    }
                    else
                    {
                        numPrefix = 9;
                        min = 144;
                    }
                }
            }
            Lvalue = new int[numPrefix];
            //debug1 += _tab(depth)+"Current Index: " +currentIndex+"\n"+_tab(depth)+"Num Prefix: " + numPrefix + ", min: " + min +" "; 
            for (int i = 0; i < numPrefix; i++)
            {
                Lvalue[numPrefix - i - 1] = list[currentIndex];
                currentIndex++;
            }

            //Length
            switch (min)
            {
                case 0:
                    min_8bit = new int[8];

                    min_8bit[7] = 0; min_8bit[6] = 0; min_8bit[5] = 1; min_8bit[4] = 1; min_8bit[3] = 0; min_8bit[2] = 0; min_8bit[1] = 0; min_8bit[0] = 0;
                    diff = binary2int(Lvalue, 8) - binary2int(min_8bit, 8);
                    wordCode = diff + min;
                    break;
                case 144:
                    min_8bit = new int[9];
                    min_8bit[8] = 1; min_8bit[7] = 1; min_8bit[6] = 0; min_8bit[5] = 0; min_8bit[4] = 1; min_8bit[3] = 0; min_8bit[2] = 0; min_8bit[1] = 0; min_8bit[0] = 0;

                    diff = binary2int(Lvalue, 9) - binary2int(min_8bit, 9);
                    wordCode = diff + min;
                    break;
                case 256:
                    min_8bit = new int[7];
                    min_8bit[6] = 0; min_8bit[5] = 0; min_8bit[4] = 0; min_8bit[3] = 0; min_8bit[2] = 0; min_8bit[1] = 0; min_8bit[0] = 0;

                    diff = binary2int(Lvalue, 7) - binary2int(min_8bit, 7);
                    wordCode = diff + min;
                    break;
                case 280:
                    min_8bit = new int[8];
                    min_8bit[7] = 1; min_8bit[6] = 1; min_8bit[5] = 0; min_8bit[4] = 0; min_8bit[3] = 0; min_8bit[2] = 0; min_8bit[1] = 0; min_8bit[0] = 0;

                    diff = binary2int(Lvalue, 8) - binary2int(min_8bit, 8);
                    wordCode = diff + min;
                    break;

                default:
                    diff = -1;
                    wordCode = 0;
                    break;
            }

            if (diff < 0)
            {
                diff = diff;
            }
            else if (wordCode < 0 || wordCode > 285)
            {
                wordCode = wordCode;
            }
            if (0 <= wordCode && wordCode < 256)
            {
                extraLengthBit = -1;
                lengthAccord = -1;
                Codes.Add(wordCode);
                //               debug1 += wordCode.ToString("X2") +"* ";
            }
            else if (wordCode == 256)
            {
                // debug1 += "END\n";
                extraLengthBit = -50;
                lengthAccord = -50;
                break;

            }
            else if (257 <= wordCode && wordCode < 265)
            {
                lengthAccord = wordCode - 254;
                extraLengthBit = 0;
            }
            else if (265 <= wordCode && wordCode < 269)
            {
                int difference = wordCode - 265;
                lengthAccord = 11 + difference * 2;
                extraLengthBit = 1;
                lengthAccord += list[currentIndex];
                currentIndex += extraLengthBit;
            }
            else if (269 <= wordCode && wordCode < 273)
            {
                int difference = wordCode - 269;
                lengthAccord = 19 + difference * 4;
                extraLengthBit = 2;
                lengthAccord += list[currentIndex] + list[currentIndex + 1] * 2;
                currentIndex += extraLengthBit;
            }
            else if (273 <= wordCode && wordCode < 277)
            {
                int difference = wordCode - 273;
                lengthAccord = 35 + difference * 8;
                extraLengthBit = 3;
                lengthAccord += list[currentIndex] + list[currentIndex + 1] * 2 + list[currentIndex + 2] * 4;
                currentIndex += extraLengthBit;
            }
            else if (277 <= wordCode && wordCode < 281)
            {
                int difference = wordCode - 277;
                lengthAccord = 67 + difference * 8;
                extraLengthBit = 4;
                lengthAccord += list[currentIndex] + list[currentIndex + 1] * 2 + list[currentIndex + 2] * 4 + list[currentIndex + 3] * 8;
                currentIndex += extraLengthBit;
            }
            else if (281 <= wordCode && wordCode < 285)
            {
                int difference = wordCode - 281;
                lengthAccord = 131 + difference * 16;
                extraLengthBit = 5;
                lengthAccord += list[currentIndex] + list[currentIndex + 1] * 2 + list[currentIndex + 2] * 4 + list[currentIndex + 3] * 8 + list[currentIndex + 4] * 16;
                currentIndex += extraLengthBit;
            }
            else if (wordCode == 285)
            {
                extraLengthBit = 0;
                lengthAccord = 258;
            }
            else
            {
                lengthAccord = -100;
                extraLengthBit = -100;
            }
            // debug1 += "wordCode: " + wordCode + ", Extra Length Bit: " + extraLengthBit +"\n" ;
            if (wordCode > 256)
            {
                Codes.Add(-1);
                //  debug1 +="(" + lengthAccord + ",";
                //Distance
                Dvalue = new int[5];
                for (int i = 0; i < 5; i++)
                {
                    Dvalue[4 - i] = list[currentIndex];
                    currentIndex++;
                }
                distCode = binary2int(Dvalue, 5);

                if (0 <= distCode && distCode < 4)
                {
                    extraDistBit = 0;
                    // debug1 += distCode+1 + ") ";
                    Codes.Add(lengthAccord);
                    Codes.Add(distCode + 1);
                    Codes.Add(-2);
                    continue;
                }
                else
                {

                    extraDistBit = ((distCode - 3) / 2);
                    if (distCode % 2 == 0)
                    {
                        extraDistBit++;
                    }
                }


                distance = (int)Mathf.Pow(2, extraDistBit) * (distCode % 2 + 2) + 1;


                for (int i = 0; i < extraDistBit; i++)
                {
                    distance += list[currentIndex + i] * (int)Mathf.Pow(2, i);
                }
                currentIndex += extraDistBit;
                //  debug1 += distance + ") ";
                Codes.Add(lengthAccord);
                Codes.Add(distance);
                Codes.Add(-2);
                //debug1 += "Extra distance bit: " + extraDistBit;
            }
        }
        if (!IsFinal)
        {
            BlockHeader(list, currentIndex);
            /*
            int BFINAL = list[currentIndex];
            int[] BTYPE_2bit = { list[currentIndex + 1], list[currentIndex + 2] };
            int BTYPE = 2 * BTYPE_2bit[1] + BTYPE_2bit[0];
            bool NextIsFinal = (BFINAL == 0) ? false : true;
            if (BTYPE == 1)
            {
                FixedHuffman(list, currentIndex + 3, NextIsFinal);
            }
            else if (BTYPE == 2)
            {
                CustomHuffman(list, currentIndex + 3, NextIsFinal);
            }
            */
        }
        debug1 += "\n";
    }


    #endregion

    #region decimal
    /// <summary>
    /// byte(int) -> hexadecimal(two digits) -> int
    /// 169 -> a9 ->  (not necessary)
    /// 
    /// </summary>
    /// <param name="octet"></param>
    /// <returns></returns>
    int decimalVersion(byte octet)
    {

        int[] digit16 = new int[2];
        int result = 0;
        // 169 ->   digit16[1] = a(10)
        digit16[1] = Mathf.FloorToInt(octet / 16);
        // 169 ->   digit16[0] = 9
        digit16[0] = octet - digit16[1] * 16;
        print(digit16[1] + "," + digit16[0]);

        for (int i = 0; i < 2; i++)
        {
            result += digit16[i] * (int)Mathf.Pow(16, i);
        }
        return result;
    }
    /// <summary>
    /// byte(int) -> hexadecimal(four digits) -> int
    /// 169 20 -> a9 14 -> 14 a9 -> 1*16^3 + 4*16^2 + 10*16^1 + 9
    /// ATTENTION TO THE ORDER
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="n"></param>
    /// <returns></returns>
    int decimalVersion2(byte[] bytes, int n)
    {
        int[] digit16 = new int[4];
        int result = 0;

        digit16[3] = Mathf.FloorToInt(bytes[n + 1] / 16);
        digit16[2] = bytes[n + 1] - digit16[3] * 16;
        digit16[1] = Mathf.FloorToInt(bytes[n] / 16);
        digit16[0] = bytes[n] - digit16[1] * 16;
        print(digit16[3] + "," + digit16[2] + "," + digit16[1] + "," + digit16[0]);

        for (int i = 0; i < 4; i++)
        {
            result += digit16[i] * (int)Mathf.Pow(16, i);
        }
        return result;
    }
    /// <summary>
    /// byte(int) -> hexadecimal(8 digits) -> int
    /// 5 0 0 0 -> 05 00 00 00  -> 00 00 00 05 -> 5
    /// ATTENTION TO THE ORDER
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="n"></param>
    /// <returns></returns>
    int decimalVersion4(byte[] bytes, int n)

    {
        int[] digit16 = new int[8];
        int result = 0;
        if (n + 3 < fileLen)
        {
            digit16[7] = Mathf.FloorToInt(bytes[n + 3] / 16);
            digit16[6] = bytes[n + 3] - digit16[7] * 16;
        }
        if (n + 2 < fileLen)
        {
            digit16[5] = Mathf.FloorToInt(bytes[n + 2] / 16);
            digit16[4] = bytes[n + 2] - digit16[5] * 16;
        }
        if (n + 1 < fileLen)
        {
            digit16[3] = Mathf.FloorToInt(bytes[n + 1] / 16);
            digit16[2] = bytes[n + 1] - digit16[3] * 16;
        }
        if (n < fileLen)
        {
            digit16[1] = Mathf.FloorToInt(bytes[n] / 16);
            digit16[0] = bytes[n] - digit16[1] * 16;
        }
        print(digit16[7] + "," + digit16[6] + "," + digit16[5] + "," + digit16[4] + "," + digit16[3] + "," + digit16[2] + "," + digit16[1] + "," + digit16[0]);

        for (int i = 0; i < 8; i++)
        {
            result += digit16[i] * (int)Mathf.Pow(16, i);
        }
        return result;
    }
    int decimalVersion8(byte[] bytes, int n)
    {
        int[] digit16 = new int[16];
        int result = 0;


        for (int i = 0; i < 16; i++)
        {
            result += (int)(digit16[i] * (int)Mathf.Pow(16, i));
        }
        return result;
    }
    #endregion

    List<int> addBinaryList(List<int> list, byte octet)

    {
        int[] octetBinary = byte2Binairy(octet);
        for (int i = 0; i < octetBinary.Length; i++)
        {
            list.Add(octetBinary[i]);
        }
        return list;
    }
    #region Convert
    int[] byte2Binairy(byte _octet)
    {
        int octet = _octet;
        int[] bytes = new int[8];
        for (int i = 0; i < 8; i++)
        {
            bytes[i] = octet % 2;
            octet = Mathf.FloorToInt(octet / 2);
        }
        return bytes;

    }
    double binary2double(int[] binaries)
    {
        double result;
        int binaryLength = binaries.Length;
        int exp = 0;
        double fraction = 0.0d;
        if (binaryLength == 64)
        {
            for (int i = 52; i < 63; i++)
            {
                exp += binaries[i] * (int)Mathf.Pow(2, i - 52);
            }
            exp += -1023;
            for (int j = 0; j < 52; j++)
            {
                fraction += binaries[j] * (double)Mathf.Pow(2, j - 52);
            }
            fraction += 1.0;

            result = fraction * Mathf.Pow(2, exp);

            result *= Mathf.Pow(-1, binaries[63]);

        }
        else
        {
            for (int i = 23; i < 31; i++)
            {
                exp += binaries[i] * (int)Mathf.Pow(2, i - 23);
            }
            exp += -127;
            for (int j = 0; j < 23; j++)
            {
                fraction += binaries[j] * (double)Mathf.Pow(2, j - 23);
            }
            fraction += 1.0;

            result = fraction * (double)Mathf.Pow(2, exp);

            result *= Mathf.Pow(-1, binaries[31]);
        }
        if (Mathf.Abs((float)result) < Mathf.Pow(10, -10))
        {
            return 0;
        }
        else
        {
            return result;
        }

    }
    #region binary2int
    /*
     |2^(n-1)|2^(n-2)|...|2^2|2^1|2^0|
     |  n-1  |  n-2  |...| 2 | 1 | 0 |
     */
    int binary2int(int[] binaries, int n)
    {
        int result;
        result = 0;
        for (int i = 0; i < n; i++)
        {
            result += binaries[i] * (int)Mathf.Pow(2, i);
        }

        return result;
    }
    int binary2int(int[] binaries, int startIndex, int n)
    {
        int result;
        result = 0;
        for (int i = startIndex; i < startIndex + n; i++)
        {
            result += binaries[i] * (int)Mathf.Pow(2, i - startIndex);
        }

        return result;
    }
    int binary2int(int[] binaries)
    {
        int n = binaries.Length;
        int result;
        result = 0;
        for (int i = 0; i < n; i++)
        {
            result += binaries[i] * (int)Mathf.Pow(2, i);
        }

        return result;
    }
    #endregion
    /*
 |2^63|2^62|...|2^1|2^0|
 | 63 | 30 |...| 1 | 0 |
 */
    long binary2long(int[] binaries)
    {
        long result;
        result = 0;
        for (int i = 0; i < 64; i++)
        {
            result += binaries[i] * (int)Mathf.Pow(2, i);
        }

        return result;
    }
    string Byte2Str(byte octet)
    {
        if (octet >= 48 && octet <= 57)
        {
            return (octet - 48).ToString();
        }
        else if (octet >= 65 && octet <= 90)
        {
            return System.Convert.ToChar(octet).ToString();
        }
        else if (octet >= 97 && octet <= 122)
        {
            return System.Convert.ToChar(octet).ToString();
        }
        else
        {
            switch (octet)
            {
                case 0:
                    return "00";
                case 11:
                    return "\\v";
                case 13:
                    return "\\r";
                case 32:
                    return " ";
                case 40:
                    return "(";
                case 41:
                    return ")";
                case 43:
                    return "+";
                case 44:
                    return ",";
                case 45:
                    return "-";
                case 46:
                    return ".";
                case 47:
                    return "/";
                case 58:
                    return ":";
                case 63:
                    return "?";

                case 92:
                    return "\\";
                case 95:
                    return "-";
                case 124:
                    return "|";
                default:
                    return octet.ToString();

            }
        }
    }
    #endregion

    #region hexadocimal
    string hexadecimal(int _int)
    {
        return "0x" + _int.ToString("x");
    }
    string hexadecimal(long _long)
    {
        return "0x" + _long.ToString("x");
    }

    #endregion
 



   
}
