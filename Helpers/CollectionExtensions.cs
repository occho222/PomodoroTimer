using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PomodoroTimer.Helpers
{
    /// <summary>
    /// コレクション操作の拡張メソッド
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// ObservableCollectionの内容を新しいアイテムで置き換える
        /// </summary>
        /// <typeparam name="T">要素の型</typeparam>
        /// <param name="collection">対象のコレクション</param>
        /// <param name="newItems">新しいアイテム</param>
        public static void ReplaceWith<T>(this ObservableCollection<T> collection, IEnumerable<T> newItems)
        {
            collection.Clear();
            foreach (var item in newItems)
            {
                collection.Add(item);
            }
        }
        
        /// <summary>
        /// ObservableCollectionの内容を新しいアイテムで置き換える（配列版）
        /// </summary>
        /// <typeparam name="T">要素の型</typeparam>
        /// <param name="collection">対象のコレクション</param>
        /// <param name="newItems">新しいアイテム</param>
        public static void ReplaceWith<T>(this ObservableCollection<T> collection, params T[] newItems)
        {
            collection.Clear();
            foreach (var item in newItems)
            {
                collection.Add(item);
            }
        }
        
        /// <summary>
        /// ObservableCollectionに複数のアイテムを追加する
        /// </summary>
        /// <typeparam name="T">要素の型</typeparam>
        /// <param name="collection">対象のコレクション</param>
        /// <param name="items">追加するアイテム</param>
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
        
        /// <summary>
        /// 指定した条件に一致するアイテムをすべて削除する
        /// </summary>
        /// <typeparam name="T">要素の型</typeparam>
        /// <param name="collection">対象のコレクション</param>
        /// <param name="predicate">削除条件</param>
        /// <returns>削除されたアイテムの数</returns>
        public static int RemoveWhere<T>(this ObservableCollection<T> collection, System.Func<T, bool> predicate)
        {
            var itemsToRemove = collection.Where(predicate).ToList();
            foreach (var item in itemsToRemove)
            {
                collection.Remove(item);
            }
            return itemsToRemove.Count;
        }
    }
}