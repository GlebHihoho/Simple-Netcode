namespace Players
{
    public class PlugInput : IInput
    {
        public float Direction()
        {
            return 0;
        }


        public bool Jump()
        {
            return false;
        }


        public bool Shoot()
        {
            return false;
        }
    }
}