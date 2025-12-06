# Audio Manager - Hướng dẫn sử dụng

## Tính năng chính

✅ **Multiple Audio Clips**: Mỗi loại âm thanh có thể chứa nhiều file khác nhau
✅ **Random Playback**: Tự động phát ngẫu nhiên từ danh sách clips
✅ **Pitch Variation**: Thay đổi pitch ngẫu nhiên mỗi lần phát
✅ **Settings Integration**: Tích hợp với SettingsManager để tắt/bật âm thanh
✅ **Fade In/Out**: Chuyển đổi âm thanh mượt mà
✅ **Music Management**: Quản lý nhạc nền riêng biệt

## Cách sử dụng AudioManager

### 1. Setup trong Unity Editor
1. Tạo một GameObject mới trong scene (ví dụ: "AudioManager")
2. Thêm component `AudioManager` vào GameObject đó
3. Trong Inspector, thêm các Sound vào mảng `Sounds`:
   - Chọn **Audio Type** (Shoot, Hit, UITap, Victory, Failed, v.v.)
   - **Thêm nhiều Audio Clips** vào mảng Clips (kéo thả nhiều file cùng lúc)
   - Điều chỉnh **Volume** (0-1)
   - Điều chỉnh **Pitch** (0.1-3)
   - Điều chỉnh **Pitch Variation** (0-0.5) để tạo sự đa dạng
   - Đánh dấu **Loop** nếu cần (cho nhạc nền)
   - Đánh dấu **Is Music** nếu là nhạc nền

**Ví dụ setup cho âm thanh Shoot:**
```
Audio Type: Shoot
Clips: [shoot1.wav, shoot2.wav, shoot3.wav, shoot4.wav]
Volume: 0.8
Pitch: 1.0
Pitch Variation: 0.1  // Mỗi lần phát sẽ có pitch từ 0.9 đến 1.1
Loop: false
Is Music: false
```

### 2. Sử dụng trong Code

#### Phát âm thanh cơ bản:
```csharp
// Phát âm thanh shoot (random từ danh sách clips)
AudioManager.Instance.Play(AudioType.Shoot);

// Phát âm thanh UI tap (random + pitch variation)
AudioManager.Instance.Play(AudioType.UITap);

// Phát âm thanh victory
AudioManager.Instance.Play(AudioType.Victory);
```

**Lưu ý:** Mỗi lần gọi `Play()` hoặc `PlayOneShot()`:
- Sẽ random chọn 1 clip từ danh sách clips
- Sẽ random pitch trong khoảng `[pitch - pitchVariation, pitch + pitchVariation]`
- Tạo sự đa dạng và tự nhiên hơn cho âm thanh

#### Phát âm thanh một lần (OneShot):
```csharp
// Tốt cho âm thanh ngắn, không cần dừng
// Mỗi lần gọi sẽ phát một clip khác nhau
AudioManager.Instance.PlayOneShot(AudioType.Hit);
AudioManager.Instance.PlayOneShot(AudioType.Collect);

// Ví dụ: Bắn liên tục
for (int i = 0; i < 5; i++)
{
    AudioManager.Instance.PlayOneShot(AudioType.Shoot);
    // Mỗi phát sẽ có âm thanh hơi khác nhau
}
```

#### Phát âm thanh với volume tùy chỉnh:
```csharp
// Phát với 50% volume
AudioManager.Instance.PlayWithVolume(AudioType.Explosion, 0.5f);
```

#### Quản lý nhạc nền:
```csharp
// Phát nhạc menu
AudioManager.Instance.PlayMusic(AudioType.MenuMusic);

// Phát nhạc gameplay với fade in
AudioManager.Instance.PlayMusic(AudioType.GameplayMusic, fadeIn: true, fadeDuration: 2f);

// Chuyển đổi nhạc mượt mà
AudioManager.Instance.CrossfadeMusic(AudioType.MenuMusic, AudioType.GameplayMusic, 1.5f);
```

#### Điều khiển âm thanh:
```csharp
// Dừng âm thanh
AudioManager.Instance.Stop(AudioType.GameplayMusic);

// Tạm dừng
AudioManager.Instance.Pause(AudioType.GameplayMusic);

// Tiếp tục
AudioManager.Instance.Resume(AudioType.GameplayMusic);

// Kiểm tra đang phát
if (AudioManager.Instance.IsPlaying(AudioType.GameplayMusic))
{
    Debug.Log("Nhạc đang phát");
}
```

#### Fade in/out:
```csharp
// Fade in trong 2 giây
AudioManager.Instance.FadeIn(AudioType.GameplayMusic, 2f);

// Fade out trong 1 giây
AudioManager.Instance.FadeOut(AudioType.GameplayMusic, 1f);
```

#### Dừng tất cả:
```csharp
// Dừng tất cả âm thanh (không bao gồm nhạc)
AudioManager.Instance.StopAllSounds();

// Dừng tất cả nhạc
AudioManager.Instance.StopAllMusic();

// Dừng tất cả
AudioManager.Instance.StopAll();
```

### 3. Ví dụ thực tế

#### Trong script của súng:
```csharp
public class Gun : MonoBehaviour
{
    public void Shoot()
    {
        // Logic bắn
        AudioManager.Instance.PlayOneShot(AudioType.Shoot);
    }
}
```

#### Trong script của enemy:
```csharp
public class Enemy : MonoBehaviour
{
    public void TakeDamage()
    {
        // Logic nhận damage
        AudioManager.Instance.PlayOneShot(AudioType.Hit);
    }
    
    public void Die()
    {
        // Logic chết
        AudioManager.Instance.PlayOneShot(AudioType.Explosion);
    }
}
```

#### Trong UI Button:
```csharp
public class UIButton : MonoBehaviour
{
    public void OnClick()
    {
        AudioManager.Instance.PlayOneShot(AudioType.UITap);
        // Logic button
    }
}
```

#### Trong Game Manager:
```csharp
public class GameManager : MonoBehaviour
{
    private void Start()
    {
        // Phát nhạc menu khi bắt đầu
        AudioManager.Instance.PlayMusic(AudioType.MenuMusic);
    }
    
    public void StartGame()
    {
        // Chuyển sang nhạc gameplay
        AudioManager.Instance.CrossfadeMusic(AudioType.MenuMusic, AudioType.GameplayMusic, 1f);
    }
    
    public void Victory()
    {
        AudioManager.Instance.StopAllMusic();
        AudioManager.Instance.Play(AudioType.Victory);
    }
    
    public void GameOver()
    {
        AudioManager.Instance.StopAllMusic();
        AudioManager.Instance.Play(AudioType.Failed);
    }
}
```

### 4. Tích hợp với Settings

AudioManager tự động tích hợp với SettingsManager:
- Khi người chơi tắt Sound trong Settings, tất cả âm thanh sẽ không phát
- Khi người chơi tắt Music trong Settings, tất cả nhạc nền sẽ không phát
- Khi bật lại, âm thanh sẽ hoạt động bình thường

```csharp
// Settings sẽ tự động cập nhật AudioManager
SettingsManager.Instance.ToggleSound(); // Tắt/bật sound
SettingsManager.Instance.ToggleMusic(); // Tắt/bật music
```

### 5. Các AudioType có sẵn

**UI Sounds:**
- UITap, UIClick, UIOpen, UIClose

**Game Sounds:**
- Shoot, Hit, Explosion, Jump, Collect, PowerUp

**Result Sounds:**
- Victory, Failed, LevelComplete

**Background Music:**
- MenuMusic, GameplayMusic, BossMusic

Bạn có thể thêm nhiều loại âm thanh khác trong file `AudioType.cs`.

### 6. Tips

1. **Âm thanh ngắn**: Dùng `PlayOneShot()` cho hiệu suất tốt hơn
2. **Nhạc nền**: Đánh dấu `Loop = true` và `Is Music = true`
3. **Fade**: Sử dụng fade cho chuyển cảnh mượt mà
4. **Volume**: Điều chỉnh Master Volume để thay đổi tất cả âm thanh
5. **Performance**: Dùng Object Pooling nếu phát quá nhiều âm thanh cùng lúc
6. **Multiple Clips**: Thêm 3-5 biến thể cho mỗi loại âm thanh để tạo sự đa dạng
7. **Pitch Variation**: Đặt 0.05-0.15 cho âm thanh tự nhiên hơn
8. **Random Tips**:
   - Shoot sounds: 3-4 clips với pitch variation 0.1
   - Hit sounds: 4-5 clips với pitch variation 0.15
   - UI sounds: 2-3 clips với pitch variation 0.05
   - Explosion: 3-4 clips với pitch variation 0.2

## Ví dụ Setup Thực Tế

### Âm thanh Shoot (Gun)
```
Audio Type: Shoot
Clips: [gun_shot_1.wav, gun_shot_2.wav, gun_shot_3.wav]
Volume: 0.7
Pitch: 1.0
Pitch Variation: 0.1  // Tạo âm thanh từ 0.9 đến 1.1
```
**Kết quả**: Mỗi lần bắn sẽ có âm thanh hơi khác nhau, tự nhiên hơn

### Âm thanh Hit (Impact)
```
Audio Type: Hit
Clips: [impact_1.wav, impact_2.wav, impact_3.wav, impact_4.wav]
Volume: 0.8
Pitch: 1.0
Pitch Variation: 0.15  // Biến thể nhiều hơn
```

### Âm thanh UI Tap
```
Audio Type: UITap
Clips: [ui_click_1.wav, ui_click_2.wav]
Volume: 0.6
Pitch: 1.0
Pitch Variation: 0.05  // Biến thể nhẹ
```

### Nhạc nền Gameplay
```
Audio Type: GameplayMusic
Clips: [bgm_gameplay.mp3]  // Chỉ 1 clip cho nhạc nền
Volume: 0.5
Pitch: 1.0
Pitch Variation: 0
Loop: true
Is Music: true
```
