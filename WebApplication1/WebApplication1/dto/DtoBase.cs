using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.dto {

    /// <summary>
    /// Enumに文字列を付加するためのAttributeクラス
    /// </summary>
    public class StringValueAttribute : Attribute {
        /// <summary>
        /// Holds the stringvalue for a value in an enum.
        /// </summary>
        public string StringValue { get; protected set; }

        /// <summary>
        /// Constructor used to init a StringValue Attribute
        /// </summary>
        /// <param name="value"></param>
        public StringValueAttribute(string value) {
            this.StringValue = value;
        }
    }
    public class StatusCodeStringValueAttribute: StringValueAttribute {
        public StatusCodeStringValueAttribute(string value) : base(value) { }
    }
    public class MessageStringValueAttribute : StringValueAttribute {
        public MessageStringValueAttribute(string value) : base(value) { }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class StatusCodeAttribute {

        /// <summary>
        /// Will get the string value for a given enums value, this will
        /// only work if you assign the StringValue attribute to
        /// the items in your enum.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetStatusCode(this Enum value) {
            // Get the type
            Type type = value.GetType();

            // Get fieldinfo for this type
            System.Reflection.FieldInfo fieldInfo = type.GetField(value.ToString());

            //範囲外の値チェック
            if (fieldInfo == null) return null;

            StatusCodeStringValueAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(StatusCodeStringValueAttribute), false) as StatusCodeStringValueAttribute[];

            // Return the first if there was a match.
            return attribs.Length > 0 ? attribs[0].StringValue : null;
        }
        public static string GetMessage(this Enum value) {
            // Get the type
            Type type = value.GetType();

            // Get fieldinfo for this type
            System.Reflection.FieldInfo fieldInfo = type.GetField(value.ToString());

            //範囲外の値チェック
            if (fieldInfo == null) return null;

            MessageStringValueAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(MessageStringValueAttribute), false) as MessageStringValueAttribute[];

            // Return the first if there was a match.
            return attribs.Length > 0 ? attribs[0].StringValue : null;
        }
    }
    public enum ApiStatusCode {    
            [StatusCodeStringValue("SYS-ERR0000")]
            [MessageStringValue("システムエラーが発生しました。")]
            StatusSysError,
            [StatusCodeStringValue("SYS-ERR0001")]
            [MessageStringValue("対象情報が見つかりません。")]
            StatusNotFound,
            [StatusCodeStringValue("SYS-ERR0002")]
            [MessageStringValue("入力値が不正です。")]
            StatusIllegalArg,
            [StatusCodeStringValue("APP-MSG0000")]
            [MessageStringValue("")]
            StatusOK,
    }

    /// <summary>
    /// DTO（Data Transfer Object）クラス
    /// APIの結果格納用の基底クラス
    /// </summary>
    public class DtoBase {
        /// <summary>
        /// APIのステータスコード
        /// 実行結果のコードを返却
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// APIのステータスコードに該当するメッセージ
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// APIから返却するデータ
        /// ※派生クラスで返却データのクラスを上書き
        /// </summary>
        public virtual object Data { get; set; }

    }
}