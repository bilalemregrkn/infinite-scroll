# Unity Infinite Scroll

This project provides an example of implementing Infinite Scroll in Unity. Infinite Scroll simulates endless scrolling for lists or content and uses an optimized pooling logic.

### Examples
![ezgif com-video-to-gif (1)](https://github.com/bilalemregrkn/infinite-scroll/assets/28481044/dc3de43d-0b05-4ef6-a98d-4fe8c0b67660)
![ezgif com-video-to-gif](https://github.com/bilalemregrkn/infinite-scroll/assets/28481044/77b0ff1a-7e14-4231-8850-5a04a687c32d)

## Features

- **Pooling Logic:** This Infinite Scroll example efficiently manages memory and CPU resources by reusing invisible items, enhancing performance, especially with large lists.

- **Flexibility:** This example offers a flexible structure, making it easy to customize content types and appearances to meet your specific needs.

## How to Use

1. **Download the Unity Package:**
   Download the published Unity Package.

2. **Import Unity Package:**
   Open your Unity project and drag and drop the Unity Package or double-click it. Then click the "Import" button.

3. **Use Prefab or Add the InfiniteScroll Component to a GameObject**

## Important Files and Folders

- **Scripts:**
  - `InfiniteScroll.cs`: A C# script containing the Infinite Scroll logic.
  
- **Prefabs:**
  - `BaseCell`: An example pre-configured prefab for the items displayed within the Infinite Scroll.

## Customization

1. **Changing Content:**
   - The `BaseCell` represents the content item. Replace or customize this prefab with your desired content.

2. **Altering Appearance:**
   - Modify the appearance of items by editing the `BaseCell` or creating new prefabs.

3. **Pooling Settings:**
   - Adjust the pooling settings in the `InfiniteScroll.cs` script to display more or fewer items on the screen as needed.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
