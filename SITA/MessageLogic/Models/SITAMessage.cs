using SITA.MessageLogic.Models.Enums;
using System.Text;

namespace SITA.MessageLogic.Models
{
    /// <summary>
    /// Структура сообщения, которая генерируется ситой
    /// </summary>
    public class SITAMessage
    {
        #region AppId
        /// <summary>
        /// Логин бсис, установленный в аэропорту, например в минске это LHR_BSI
        /// </summary>
        public static string? ApplicationIdentify = null!;

        /// <summary>
        /// Флаг изменения appl_id
        /// </summary>
        private static bool _isChanged;

        /// <summary>
        /// Помечает appl_id как измененный
        /// </summary>
        public static void MarkAppIdAsChanged()
        {
            _isChanged = true;
        }

        /// <summary>
        /// Идентификатор приложения в бинарном виде
        /// </summary>
        private static byte[]? _appId;

        /// <summary>
        /// Получение и изменение названия приложения
        /// Если не был установлен или из контроллера помечен как измененный, то указываем, что ApplicationIdentify нужно загрузить из базы
        /// </summary>
        public static byte[] AppId
        {
            get
            {
                if (_isChanged || _appId == null)
                {
                    _isChanged = false;
                    ApplicationIdentify = "LHR_BRS";
                    _appId = ApplicationIdentify.Select(x => (byte)x).ToArray();
                }
                return _appId;
            }
            set
            {
                _appId = value;
                ApplicationIdentify = Encoding.Default.GetString(value);
            }
        }
        #endregion
        /// <summary>
        /// Версия приложения
        /// </summary>
        public ushort Version = 2;
        #region Type
        /// <summary>
        /// Идентификатор типа сообщения
        /// </summary>
        ushort type;

        /// <summary>
        /// Тип сообщения
        /// </summary>
        public MessageType MessageType
        {
            get
            {
                return (MessageType)type;
            }
            set
            {
                type = (ushort)value;
            }
        }
        #endregion
        #region MessageIdNumber
        /// <summary>
        /// Последний использованый Id сообщения
        /// </summary>
        private static ushort _lastId = 0;

        /// <summary>
        /// Закрытое поле Id сообщения
        /// </summary>
        private ushort? _messageIdNumber = null;

        /// <summary>
        /// Id сообщения
        /// </summary>
        public ushort MessageIdNumber
        {
            get
            {
                _messageIdNumber ??= ++_lastId;

                return _messageIdNumber.Value;
            }
            set
            {
                _messageIdNumber = value;
            }
        }
        #endregion
        /// <summary>
        /// Колисчество байтов в сообщении
        /// </summary>
        public ushort DataLength => (ushort)Content.Length;

        /// <summary>
        /// Pарезервированнык байты
        /// </summary>
        public byte[] Reserved = new byte[] { 0x0, 0x0, 0x0, 0x0 };
        #region Content

        /// <summary>
        /// Сообщение в виде набора байт
        /// </summary>
        byte[] Content = Array.Empty<byte>();

        /// <summary>
        /// Закрытое поле сообщение
        /// </summary>
        string? _content_text;

        /// <summary>
        /// Сообщение
        /// </summary>
        public string ContentText
        {
            get
            {
                _content_text ??= Encoding.UTF8.GetString(Content);
                return _content_text;
            }
            set
            {
                _content_text = null;
                Content = Encoding.UTF8.GetBytes(value);
            }
        }
        #endregion
        /// <summary>
        /// Конструктор с полями
        /// </summary>
        /// <param name="content">
        /// Сообщение
        /// </param>
        /// <param name="type">
        /// Тип Сообщения
        /// </param>
        /// <returns>
        /// Объект SITAMessage
        /// </returns>
        public static SITAMessage Create(byte[] content, MessageType type = MessageType.DATA)
        {
            var obj = new SITAMessage
            {
                MessageType = type,
                Content = content
            };

            return obj;
        }

        /// <summary>
        /// Конструктор из массива байт
        /// </summary>
        /// <param name="fullData">
        /// массив байт
        /// HEDER + content
        /// </param>
        /// <returns>
        /// Объект SITAMessage
        /// </returns>
        public static SITAMessage Create(byte[] fullData)
        {
            ushort data_length = BytesToUShort(fullData, 14);
            byte[] content = new byte[data_length];
            byte[] reserved = new byte[4];

            Array.Copy(fullData, 16, reserved, 0, 4);
            Array.Copy(fullData, 20, content, 0, data_length);

            return new SITAMessage
            {
                Version = BytesToUShort(fullData, 8),
                type = BytesToUShort(fullData, 10),
                MessageIdNumber = BytesToUShort(fullData, 12),
                Reserved = reserved,
                Content = content
            };
        }

        /// <summary>
        /// Сериализация сообщения
        /// </summary>
        /// <returns></returns>
        public byte[] GetByteData()
        {
            byte[] result = new byte[20 + DataLength];

            Array.Copy(AppId, 0, result, 0, AppId.Length);
            Array.Copy(UShortToBytes(Version), 0, result, 8, 2);
            Array.Copy(UShortToBytes(type), 0, result, 10, 2);
            Array.Copy(UShortToBytes(MessageIdNumber), 0, result, 12, 2);
            Array.Copy(UShortToBytes(DataLength), 0, result, 14, 2);
            Array.Copy(Reserved, 0, result, 16, Reserved.Length);
            Array.Copy(Content, 0, result, 20, Content.Length);

            return result;
        }

        /// <summary>
        /// Получить массив байт из unsigned short
        /// </summary>
        /// <param name="value">
        /// Числовое представление
        /// </param>
        /// <returns>
        /// Представление в виде массива байт
        /// </returns>
        private static byte[] UShortToBytes(ushort value) => new byte[]{
            (byte)value,
            (byte)(value >> 8)
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// Поток байт
        /// <param name="pos"></param>
        /// Позиция в массиве байт
        /// <returns>
        /// Числовое представление
        /// </returns>
        private static ushort BytesToUShort(byte[] bytes, int pos)
            => (ushort)(bytes[pos + 1] << 8 | bytes[pos]);
    }
}
