using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using Shared;

namespace NetCore.Messages
{
    public abstract class Message
        : ISerialisable
    {
        private static Type[] MessageTypes { get; set; }

        public virtual void Serialise(IWriter Writer)
        {
            Writer.Write((byte)0); // Send a single byte as a notification of the message coming (needed for the async style reception).
            Writer.Write(GetType().Name);
        }
        public abstract void Deserialise(IReader Reader);

        protected virtual int GetMessageSize()
        {
            return NetReader.NetworkLength((byte)0) + NetReader.NetworkLength(GetType().Name);
        }

        public static void RegenMessageTypes() // Only call if a new assembly has been loaded
        {
            MessageTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(Message))).ToArray();
        }
        public static Message ReadMessage(NetReader Reader)
        {
            try
            {
                if (MessageTypes == null)
                    RegenMessageTypes();

                string Id = Reader.ReadString();

                foreach (Type t in MessageTypes)
                {
                    if (t.Name == Id)
                    {
                        Message m = (Message)Activator.CreateInstance(t);
                        m.Deserialise(Reader);
                        return m;
                    }
                }

                return null;
            }
            catch (MissingMethodException e)
            {
                throw new Exception("All Message subclasses must define a public parameterless constructor.", e);
            }
        }
        public static T ReadMessage<T>(NetReader Reader)
            where T : Message
        {
            try
            {
                string Type = Reader.ReadString();
                T Msg = Activator.CreateInstance<T>();
                if (Msg.GetType().Name != Type)
                    throw new Exception();

                Msg.Deserialise(Reader);
                return Msg;
            }
            catch
            {
                throw new Exception("Invalid message received.");
            }
        }
    }

    public static class MessageExtensions
    {
        public static void Write(this NetWriter Writer, Message Message)
        {
            Message.Serialise(Writer);
        }
    }
}
