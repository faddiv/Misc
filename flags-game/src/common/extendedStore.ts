
interface TableRow {
  id: number;
}

interface TableSchema<TRow extends TableRow> {
  id_generator: number;
  data: TRow[];
}

function getTable<TRow extends TableRow>(tableName: string): TableSchema<TRow> {
  var json = localStorage.getItem(tableName);
  if (json === null)
    return {
      id_generator: 0,
      data: []
    };;
  return JSON.parse(json) as TableSchema<TRow>;
}

function clone<T>(item: T) {
  return JSON.parse(JSON.stringify(item)) as T;
}

export function extTable<TRow extends TableRow>(tableName: string) {
  const tableData = getTable<TRow>(tableName);

  function getNextId() {
    return ++tableData.id_generator;
  }

  return {
    getKeys() {
      return tableData.data.map(e => e.id);
    },
    getElementByKey(id: number) {
      const item = tableData.data.find(e => e.id === id);
      if (item)
        return clone(item);
      else
        return null;
    },
    saveElement(element: TRow) {
      if (element.id) {
        const index = tableData.data.findIndex(e => e.id === element.id);
        if(index !== -1) {
           tableData.data[index] = clone(element);
        } else {
          tableData.data.push(clone(element));
        }
        localStorage.setItem(tableName, JSON.stringify(tableData));
        return element.id;
      }
      var newElement = clone(element);
      newElement.id = getNextId();
      tableData.data.push(newElement);
      localStorage.setItem(tableName, JSON.stringify(tableData));
      return newElement.id;
    },
    getAll() {
      // if any element is saved then it is in the storage,
      // if not then it is empty.
      return getTable<TRow>(tableName);
    },
    isEmpty() {
      return tableData.data.length === 0
    }
  }
}
