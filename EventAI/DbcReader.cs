﻿using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EventAI
{
    static class DBCReader
    {
        public const int MAX_DBC_LOCALE = 16;
        public const string DBC_PATH = @"dbc\";

        public static void Run()
        {
            //Dictionary<uint, string> nullStringDict = null;

            //DBC.Spell = DBCReader.ReadDBC<SpellEntry>(DBC_PATH + "Spell.dbc", DBC._SpellStrings);
            //DBC.SpellRadius = DBCReader.ReadDBC<SpellRadiusEntry>(DBC_PATH + "SpellRadius.dbc", nullStringDict);
            //DBC.SpellRange = DBCReader.ReadDBC<SpellRangeEntry>(DBC_PATH + "SpellRange.dbc", DBC._SpellRangeStrings);
            //DBC.SpellDuration = DBCReader.ReadDBC<SpellDurationEntry>(DBC_PATH + "SpellDuration.dbc", nullStringDict);
            //DBC.SkillLineAbility = DBCReader.ReadDBC<SkillLineAbilityEntry>(DBC_PATH + "SkillLineAbility.dbc", nullStringDict);
            //DBC.SkillLine = DBCReader.ReadDBC<SkillLineEntry>(DBC_PATH + "SkillLine.dbc", DBC._SkillLineStrings);
            //DBC.SpellCastTimes = DBCReader.ReadDBC<SpellCastTimesEntry>(DBC_PATH + "SpellCastTimes.dbc", nullStringDict);

            //// Currently we use entry 1 from Spell.dbc to detect DBC locale
            //byte DetectedLocale = 0;
            //while (DBC.Spell[1].GetName(DetectedLocale) == "")
            //{
            //    if (DetectedLocale >= MAX_DBC_LOCALE)// TODO: необходимо как-то сообщить пользователю о том, что ДБЦ у него неправильные
            //        throw new Exception("Detected unknown locale index " + DetectedLocale);
            //    ++DetectedLocale;
            //}

            //DBC.Locale = (LocalesDBC)DetectedLocale;
        }

        public static unsafe Dictionary<uint, T> ReadDBC<T>(string fileName, Dictionary<uint, string> strDict) where T : struct
        {
            Dictionary<uint, T> dict = new Dictionary<uint, T>();

            if (!File.Exists(fileName))
                throw new Exception("File " + fileName + " not found");

            BinaryReader reader = new BinaryReader(new FileStream(fileName, FileMode.Open, FileAccess.Read), Encoding.UTF8);

            // Sanity check
            if (reader.BaseStream.Length < 20 || reader.ReadUInt32() != 0x43424457)
                throw new Exception(String.Format("Bad DBC file {0}", fileName));

            int recordsCount = reader.ReadInt32();
            int fieldsCount = reader.ReadInt32();
            int recordSize = reader.ReadInt32();
            int stringTableSize = reader.ReadInt32();

            int size = Marshal.SizeOf(typeof(T));

            if (recordSize != size)// TODO: необходимо как-то сообщить пользователю о том, что ДБЦ у него возможно не той версии
                throw new Exception(String.Format("\n\nSize of row in DBC file ({0}) != size of DBC struct ({1})\nDBC: {2}\n\n",
                    recordSize, size, fileName));

            BinaryReader dataReader = new BinaryReader(new MemoryStream(reader.ReadBytes(recordsCount * recordSize)), Encoding.UTF8);
            BinaryReader stringsReader = new BinaryReader(new MemoryStream(reader.ReadBytes(stringTableSize)), Encoding.UTF8);

            reader.Close();

            for (int r = 0; r < recordsCount; ++r)
            {
                uint key = dataReader.ReadUInt32();
                dataReader.BaseStream.Position -= 4;

                T T_entry = dataReader.ReadStruct<T>();

                dict.Add(key, T_entry);
            }

            dataReader.Close();

            // Now we read strings
            if (strDict != null)
            {
                while (stringsReader.BaseStream.Position != stringsReader.BaseStream.Length)
                {
                    var offset = (uint)stringsReader.BaseStream.Position;
                    var str = stringsReader.ReadCString();
                    strDict.Add(offset, str);
                }
            }

            stringsReader.Close();

            return dict;
        }
    }
}
