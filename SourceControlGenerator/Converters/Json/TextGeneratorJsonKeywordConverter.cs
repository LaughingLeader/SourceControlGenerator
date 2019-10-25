using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SCG.Data.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Converters.Json
{
	public class TextGeneratorJsonKeywordConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return true;
		}

		private JsonReader CreateNewReader(JsonReader reader, JObject obj)
		{
			JsonReader jObjectReader = obj.CreateReader();
			jObjectReader.Culture = reader.Culture;
			jObjectReader.DateParseHandling = reader.DateParseHandling;
			jObjectReader.DateTimeZoneHandling = reader.DateTimeZoneHandling;
			jObjectReader.FloatParseHandling = reader.FloatParseHandling;
			return jObjectReader;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null) return null;

			var obj = JObject.Load(reader);
			string inputTypeString = (string)obj.SelectToken("InputType");

			if (!String.IsNullOrWhiteSpace(inputTypeString))
			{
				TextGeneratorInputType inputType = (TextGeneratorInputType)Enum.Parse(typeof(TextGeneratorInputType), inputTypeString);
				//Log.Here().Activity($"inputType: {inputType.ToString()}");

				if (inputType == TextGeneratorInputType.Incremental || inputType == TextGeneratorInputType.Decremental)
				{
					TextGeneratorInputNumberData target = new TextGeneratorInputNumberData();

					var jObjectReader = CreateNewReader(reader, obj);
					serializer.Populate(jObjectReader, target);
					target.InputType = inputType;

					//Log.Here().Activity($"Created input: {target.GetType().ToString()} | {target.Keyword} | {target.IncrementBy}");

					return target;
				}
				else if (inputType == TextGeneratorInputType.Text)
				{
					TextGeneratorInputTextData target = new TextGeneratorInputTextData();

					var jObjectReader = CreateNewReader(reader, obj);
					serializer.Populate(jObjectReader, target);
					target.InputType = inputType;

					return target;
				}
				else
				{
					return null;
				}
			}
			else
			{
				TextGeneratorViewModel vm = new TextGeneratorViewModel();
				var jObjectReader = CreateNewReader(reader, obj);
				serializer.Populate(jObjectReader, vm);
				return vm;
			}
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			//Avoid self-referencing loops with Expando
			//Log.Here().Activity($"Object: {value.GetType().ToString()}");
			if (value is TextGeneratorInputTextData inputTextData)
			{
				dynamic fake = new System.Dynamic.ExpandoObject();
				fake.InputType = inputTextData.InputType.ToString();
				fake.Keyword = inputTextData.Keyword;
				fake.InputValue = inputTextData.InputValue;
				serializer.Serialize(writer, fake);
			}
			else if (value is TextGeneratorInputNumberData inputNumberData)
			{
				dynamic fake = new System.Dynamic.ExpandoObject();
				fake.InputType = inputNumberData.InputType.ToString();
				fake.Keyword = inputNumberData.Keyword;
				fake.StartValue = inputNumberData.StartValue.ToString();
				fake.IncrementBy = inputNumberData.IncrementBy.ToString();
				fake.NumberPadding = inputNumberData.NumberPadding.ToString();
				serializer.Serialize(writer, fake);
			}
			else
			{
				serializer.Serialize(writer, value);
			}
		}
	}
}
