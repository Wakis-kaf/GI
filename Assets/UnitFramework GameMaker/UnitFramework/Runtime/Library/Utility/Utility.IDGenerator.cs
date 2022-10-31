﻿﻿using System;

namespace UnitFramework.Utils
{
    public static partial class Utility
    {
        public static class IDGenerator
        {
            public static string GetStrGuidID()
            {
                return Guid.NewGuid().ToString();
            }

            public static int GetIntGuidID()
            {
                return Guid.NewGuid().GetHashCode() & int.MaxValue;
            }

            /// <summary>
            /// 方法一 使用随机抽取数组index中的数，填充在新的数组array中，使数组array中的数是随机的
            /// 方法一思路：用一个数组来保存索引号，先随机生成一个数组位置，然后把随机抽取到的位置的索引号取出来，
            ///             并把最后一个索引号复制到当前的数组位置，然后使随机数的上限减一，具体如：先把这100个数放在一个数组内，
            ///             每次随机取一个位置（第一次是1-100，第二次是1-99，...），将该位置的数用最后的数代替。
            /// </summary>
            public static int[] GetRandomIDArrayByDoubleArray(int length)
            {
                int seed = Guid.NewGuid().GetHashCode();
                Random radom = new Random(seed);
                int[] index = new int[length];
                for (int i = 0; i < length; i++)
                {
                    index[i] = i + 1;
                }

                int[] array = new int[length]; // 用来保存随机生成的不重复的数 
                int site = length; // 设置上限 
                int idx; // 获取index数组中索引为idx位置的数据，赋给结果数组array的j索引位置
                for (int j = 0; j < length; j++)
                {
                    idx = radom.Next(0, site - 1); // 生成随机索引数
                    array[j] = index[idx]; // 在随机索引位置取出一个数，保存到结果数组 
                    index[idx] = index[site - 1]; // 作废当前索引位置数据，并用数组的最后一个数据代替之
                    site--; // 索引位置的上限减一（弃置最后一个数据）
                }

                return array;
            }

            public static int GetRandomIDByDoubleArray(int length)
            {
                return GetRandomIDArrayByDoubleArray(length).GetHashCode();
            }

            public static string GetRandomIDStrByDoubleArray(int length)
            {
                return GetRandomIDByDoubleArray(length).ToString();
            }
        }
    }
}