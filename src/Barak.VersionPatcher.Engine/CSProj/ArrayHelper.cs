namespace Barak.VersionPatcher.Engine.CSProj
{
    public static class ArrayHelper
    {
        public static T[] ArrayAdd<T>(this T[] obj, T item)
        {
            if (obj == null)
            {
                return new T[1] { item };
            }

            T[] newArray = new T[obj.Length + 1];
            for (int i = 0; i < obj.Length; i++)
            {
                newArray[i] = obj[i];
            }
            newArray[newArray.Length - 1] = item;
            return newArray;
        }
    }
}