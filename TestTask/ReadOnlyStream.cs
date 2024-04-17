using System;
using System.IO;

namespace TestTask
{
    public class ReadOnlyStream : IReadOnlyStream
    {
        private Stream _localStream;
        private bool _isEof;
        public bool IsEof { get { return _isEof; } private set { } }

        /// <summary>
        /// Конструктор класса. 
        /// Т.к. происходит прямая работа с файлом, необходимо 
        /// обеспечить ГАРАНТИРОВАННОЕ закрытие файла после окончания работы с таковым!
        /// </summary>
        /// <param name="fileFullPath">Полный путь до файла для чтения</param>
        public ReadOnlyStream(string fileFullPath)
        {
            try
            {
                _localStream = new FileStream(fileFullPath, FileMode.Open, FileAccess.Read);
                _isEof = false;
            }
            catch (Exception ex)
            {
                _isEof = true;
            }
        }

        /// <summary>
        /// Ф-ция чтения следующего символа из потока.
        /// Если произведена попытка прочитать символ после достижения конца файла, метод 
        /// должен бросать соответствующее исключение
        /// </summary>
        /// <returns>Считанный символ.</returns>
        public char ReadNextChar()
        {
            if (IsEof || _localStream == null)
            {
                throw new InvalidOperationException("Попытка чтения за пределами файла или файл не открыт.");
            }

            if (_localStream.ReadByte() == -1)
            {
                IsEof = true;
                throw new EndOfStreamException("Конец файла...");
            }

            return (char)_localStream.ReadByte();
        }

        /// <summary>
        /// Сбрасывает текущую позицию потока на начало.
        /// </summary>
        public void ResetPositionToStart()
        {
            if (_localStream != null && _localStream.CanSeek)
            {
                _localStream.Position = 0;
                IsEof = false;
            }
        }

        public void Dispose()
        {
            if (_localStream != null)
            {
                _localStream.Close();
                _localStream = null;
            }
        }
    }
}
