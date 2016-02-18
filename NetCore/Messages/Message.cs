using System;
using System.Linq;
using System.Reflection;

using Shared;

namespace NetCore.Messages
{
    // Superclass for all types of message
    public abstract class Message
        : ISerialisable
    {
        private static Type[] MessageTypes { get; set; }

        public virtual void Serialise(Writer Writer)
        {
            // Send a single byte as a notification
            Writer.Write((byte)0);
            Writer.Write(GetType().Name);
        }
        public abstract void Deserialise(Reader Reader);

        public static void RegenMessageTypes() // Only call if a new assembly has been loaded
        {
            // Load all the types that inherit from this class in the executing assembly
            MessageTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(Message))).ToArray();
        }
        public static Message ReadMessage(NetReader Reader)
        {
            try
            {
                if (MessageTypes == null)
                    RegenMessageTypes(); // Load message classes if necessary

                string Id = Reader.ReadString(); // Read the type

                foreach (Type t in MessageTypes)
                {
                    if (t.Name == Id) // Found the right class
                    {
                        // Create an object of the class, deserialise to it and return
                        Message m = (Message)Activator.CreateInstance(t);
                        m.Deserialise(Reader);
                        return m;
                    }
                }

                throw new Exception("Invalid Type received"); // No matching class found
            }
            catch (MissingMethodException e)
            {
                // MissingMethodException is thrown if a subclass of Message can't be constructed
                throw new Exception("All Message subclasses must define a public parameterless constructor.", e);
            }
        }
        // Read a generic Message
        public static T ReadMessage<T>(NetReader Reader) where T : Message
        {
            try
            {
                // Read the Type but ignore it
                Reader.ReadString();

                // Create and deserialise
                T Msg = Activator.CreateInstance<T>();
                Msg.Deserialise(Reader);
                return Msg;
            }
            catch
            {
                throw new Exception("Invalid message received.");
            }
        }
    }
}
