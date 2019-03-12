using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System;
using Newtonsoft.Json.Linq;

namespace OpenKuka.KRL.Data.AST
{
    public enum KRLType
    {
        BOOL,
        INT,
        REAL,
        CHAR,
        ENUM,
        STRUC,
    }
    public enum DataObjectType
    {
        BOOL,
        INT,
        REAL,
        CHAR,
        ENUM,
        STRING,
        BITARRAY,
        STRUC,
    }

    public class DataObjectConcreteClassContractResolver : DefaultContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (typeof(DataObject).IsAssignableFrom(objectType) && !objectType.IsAbstract)
                return null; // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)
            return base.ResolveContractConverter(objectType);
        }
    }
    public class DataObjectConverter : JsonConverter
    {
        private static JsonSerializerSettings settings = new JsonSerializerSettings() {
            ContractResolver = new DataObjectConcreteClassContractResolver(),
            TypeNameHandling = TypeNameHandling.All
        };

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(DataObject));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            switch (jo["type"].Value<int>())
            {
                case 0:
                    return JsonConvert.DeserializeObject<BoolData>(jo.ToString(), settings);
                case 1:
                    return JsonConvert.DeserializeObject<IntData>(jo.ToString(), settings);
                case 2:
                    return JsonConvert.DeserializeObject<RealData>(jo.ToString(), settings);
                case 3:
                    return JsonConvert.DeserializeObject<CharData>(jo.ToString(), settings);
                case 4:
                    return JsonConvert.DeserializeObject<EnumData>(jo.ToString(), settings);
                case 5:
                    return JsonConvert.DeserializeObject<StringData>(jo.ToString(), settings);
                case 6:
                    return JsonConvert.DeserializeObject<BitArrayData>(jo.ToString(), settings);
                case 7:
                    return JsonConvert.DeserializeObject<StrucData>(jo.ToString(), settings);
            }
            throw new NotImplementedException();
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException(); // won't be called because CanWrite returns false
        }
    }

    //[JsonConverter(typeof(DataObjectConverter))]
    [DataContract]
    public abstract class DataObject
    {
        public string Name { get; set; }

        [DataMember(Name = "type", Order = 1)]
        public abstract DataObjectType Type { get; }
        public abstract string KRLType { get; }

        public abstract bool IsScalar { get; }
        public abstract bool IsStruc { get; }
        public abstract bool IsArray { get; }

        public override string ToString()
        {
            return Name + " " + ToStringValue();
        }
        public abstract string ToStringValue();
    }

    [DataContract]
    public class BoolData : DataObject
    {
        [DataMember(Name="value", Order=2)]
        public bool Value { get; private set; }

        public override DataObjectType Type => DataObjectType.BOOL;
        public override string KRLType => "BOOL";

        public override bool IsScalar => true;
        public override bool IsStruc => false;
        public override bool IsArray => false;

        [JsonConstructor]
        public BoolData(string value, string name)
        {
            Value = bool.Parse(value);
            Name = name;
        }

        public BoolData(string value) : this(value, "") { }

        public override string ToStringValue()
        {
            return Value ? "TRUE" : "FALSE";
        }
    }

    [DataContract]
    public class IntData : DataObject
    {
        [DataMember(Name = "value", Order = 2)]
        public int Value { get; private set; }

        public override DataObjectType Type => DataObjectType.INT;
        public override string KRLType => "INT";

        public override bool IsScalar => true;
        public override bool IsStruc => false;
        public override bool IsArray => false;

        public IntData(string value)
        {
            Value = int.Parse(value);
        }

        public override string ToStringValue()
        {
            return Value.ToString(NumberFormatInfo.InvariantInfo);
        }
    }

    [DataContract]
    public class RealData : DataObject
    {
        [DataMember(Name = "value", Order = 2)]
        public double Value { get; private set; }

        public override DataObjectType Type => DataObjectType.REAL;
        public override string KRLType => "REAL";

        public override bool IsScalar => true;
        public override bool IsStruc => false;
        public override bool IsArray => false;

        public RealData(string value)
        {
            Value = double.Parse(value, CultureInfo.InvariantCulture);
        }

        public override string ToStringValue()
        {
            return Value.ToString("G", NumberFormatInfo.InvariantInfo);
        }
    }

    [DataContract]
    public class CharData : DataObject
    {
        [DataMember(Name = "value", Order = 2)]
        public char Value { get; private set; }

        public override DataObjectType Type => DataObjectType.CHAR;
        public override string KRLType => "CHAR";

        public override bool IsScalar => true;
        public override bool IsStruc => false;
        public override bool IsArray => false;

        public CharData(string value)
        {
            Value = char.Parse(value);
        }

        public override string ToStringValue()
        {
            return "'" + Value.ToString() + "'";
        }
    }

    [DataContract]
    public class EnumData : DataObject
    {
        [DataMember(Name = "value", Order = 2)]
        public string Value { get; private set; }

        public override DataObjectType Type => DataObjectType.ENUM;
        public override string KRLType => "ENUM";

        public override bool IsScalar => true;
        public override bool IsStruc => false;
        public override bool IsArray => false;

        public EnumData(string value)
        {
            Value = value.Substring(1);
        }

        public override string ToStringValue()
        {
            return "#" + Value.ToString();
        }
    }

    [DataContract]
    public class StringData : DataObject
    {
        [DataMember(Name = "value", Order = 2)]
        public string Value { get; private set; }

        public override DataObjectType Type => DataObjectType.STRING;
        public override string KRLType => "BOOL";

        public override bool IsScalar => false;
        public override bool IsStruc => false;
        public override bool IsArray => true;

        public StringData(string value)
        {
            Value = value.Substring(1, value.Length - 2);
        }

        public override string ToStringValue()
        {
            return '"' + Value + '"';
        }
    }

    [DataContract]
    public class BitArrayData : DataObject
    {
        [DataMember(Name = "value", Order = 2)]
        public BitArray Value { get; private set; }

        public override DataObjectType Type => DataObjectType.BITARRAY;
        public override string KRLType => "BOOL";

        public override bool IsScalar => false;
        public override bool IsStruc => false;
        public override bool IsArray => true;

        public BitArrayData(string value)
        {
            Value = new BitArray(value.Length - 3);
            for (int i = 0; i < Value.Count; i++)
            {
                Value[i] = (value[i + 2] == '1');
            }
        }

        public override string ToStringValue()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("'B");
            for (int i = 0; i < Value.Count; i++)
            {
                stringBuilder.Append(Value[i] ? '1' : '0');
            }
            stringBuilder.Append("'");
            return stringBuilder.ToString();
        }
    }

    [DataContract]
    public class StrucData : DataObject
    {
        private string StrucType;

        [DataMember(Name = "value", Order = 2)]
        public Dictionary<string, DataObject> Value { get; set; }

        public override DataObjectType Type => DataObjectType.STRUC;

        [DataMember(Name = "strucType", Order = 2)]
        public override string KRLType => StrucType;
        public int Count => Value.Count;

        public override bool IsScalar => false;
        public override bool IsStruc => true;
        public override bool IsArray => false;

        [JsonConstructor]
        public StrucData(string strucType, Dictionary<string, DataObject> value)
        {
            StrucType = strucType;
            Value = value;
        }

        public StrucData(string strucType)
        {
            StrucType = strucType;
            Value = new Dictionary<string, DataObject>();
        }

        public DataObject this[string key]
        {
            get { return Value[key]; }
        }
        public void Add(DataObject dataobj)
        {
            Value.Add(dataobj.Name, dataobj);
        }

        public override string ToStringValue()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append('{');
            if (KRLType != string.Empty)
            {
                stringBuilder.Append(KRLType);
                stringBuilder.Append(": ");
            }
            bool flag = true;
            foreach(var item in Value.Values)
            {
                if (flag) 
                {
                    flag = false;
                }
                else
                {
                    stringBuilder.Append(", ");
                }
                stringBuilder.Append(item.ToString());
            }
            stringBuilder.Append('}');
            return stringBuilder.ToString();
        }
    }

    

    public class OrderedContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(System.Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization).OrderBy(p => p.PropertyName).ToList();
        }
    }
}
